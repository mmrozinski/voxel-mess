using OpenTK.Mathematics;
using VoxelGL;

namespace Voxel
{
    public class Frustum
    {
        Plane[] planes = new Plane[6];

        public enum Sides
        {
            FRUSTUM_TOP = 0, FRUSTUM_BOTTOM = 1, FRUSTUM_LEFT = 2, FRUSTUM_RIGHT = 3, FRUSTUM_NEAR = 4, FRUSTUM_FAR = 5,
        }

        public enum Intersections
        {
            FRUSTUM_OUTSIDE = 0, FRUSTUM_INTERSECT = 1, FRUSTUM_INSIDE = 2,
        }

        /// <summary>
        /// Creates a Frustum object based on the preovided matrix
        /// </summary>
        /// <param name="matrix">Matrix the frustum should be based on. This can e.g. be a projection matrix or a projection matrix combined with the view matrix</param>
        public Frustum (Matrix4 matrix)
        {
            matrix.Transpose();
            planes[0] = new Plane(matrix.M41 - matrix.M21, matrix.M42 - matrix.M22, matrix.M43 - matrix.M23, matrix.M44 - matrix.M24);
            planes[1] = new Plane(matrix.M41 + matrix.M21, matrix.M42 + matrix.M22, matrix.M43 + matrix.M23, matrix.M44 + matrix.M24);
            planes[2] = new Plane(matrix.M41 + matrix.M11, matrix.M42 + matrix.M12, matrix.M43 + matrix.M13, matrix.M44 + matrix.M14);
            planes[3] = new Plane(matrix.M41 - matrix.M11, matrix.M42 - matrix.M12, matrix.M43 - matrix.M13, matrix.M44 - matrix.M14);
            planes[4] = new Plane(matrix.M41 + matrix.M31, matrix.M42 + matrix.M32, matrix.M43 + matrix.M33, matrix.M44 + matrix.M34);
            planes[5] = new Plane(matrix.M41 - matrix.M31, matrix.M42 - matrix.M32, matrix.M43 - matrix.M33, matrix.M44 - matrix.M34);
        }

        public Intersections PointInFrustum (Vector3 point)
        {
            for (int i = 0; i < 6; i++)
            {
                if (planes[i].GetPointDistance(point) < 0)
                    return Intersections.FRUSTUM_OUTSIDE;
            }
            return Intersections.FRUSTUM_INSIDE;
        }

        public Intersections SphereInFrustum (Vector3 point, float radius)
        {
            Intersections result = Intersections.FRUSTUM_INSIDE;
            float distance;
            for (int i = 0; i < 6; i++)
            {
                distance = planes[i].GetPointDistance(point);
                if (distance < -radius)
                    return Intersections.FRUSTUM_OUTSIDE;
                else if (distance < radius)
                    result = Intersections.FRUSTUM_INTERSECT;
            }
            return result;
        }

        public Intersections CubeInFrustum(Vector3 center, float x, float y, float z)
        {
            Intersections result = Intersections.FRUSTUM_INSIDE;
            for (int i = 0; i < 6; i++)
            {
                int outside = 0;
                int inside = 0;

                if (planes[i].GetPointDistance(center + new Vector3(-x, -y, -z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(x, -y, -z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(-x, -y, z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(x, -y, z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(-x, y, -z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(x, y, -z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(-x, y, z)) < 0)
                    outside++;
                else
                    inside++;

                if (planes[i].GetPointDistance(center + new Vector3(x, y, z)) < 0)
                    outside++;
                else
                    inside++;

                if (inside == 0)
                    return Intersections.FRUSTUM_OUTSIDE;
                else if (outside != 0)
                    result = Intersections.FRUSTUM_INTERSECT;
            }
            return result;
        }

        Vector3 IntersectionPoint(Plane a, Plane b, Plane c)
        {
            Vector3 v1, v2, v3;
            float f = -Vector3.Dot(a.GetNormal(), Vector3.Cross(b.GetNormal(), c.GetNormal()));

            v1 = (a.d * (Vector3.Cross(b.GetNormal(), c.GetNormal())));
            v2 = (b.d * (Vector3.Cross(c.GetNormal(), a.GetNormal())));
            v3 = (c.d * (Vector3.Cross(a.GetNormal(), b.GetNormal())));

            Vector3 vec = new Vector3(v1.X + v2.X + v3.X, v1.Y + v2.Y + v3.Y, v1.Z + v2.Z + v3.Z);
            return vec / f;
        }

        public void PreRender(MeshRenderer renderer)
        {
            renderer.Clear();

            Vector3 p1 = IntersectionPoint(planes[0], planes[2], planes[4]);
            Vector3 p2 = IntersectionPoint(planes[0], planes[3], planes[4]);
            Vector3 p3 = IntersectionPoint(planes[1], planes[3], planes[4]);
            Vector3 p4 = IntersectionPoint(planes[1], planes[2], planes[4]);
            Vector3 p5 = IntersectionPoint(planes[0], planes[2], planes[5]);
            Vector3 p6 = IntersectionPoint(planes[0], planes[3], planes[5]);
            Vector3 p7 = IntersectionPoint(planes[1], planes[2], planes[5]);
            Vector3 p8 = IntersectionPoint(planes[1], planes[3], planes[5]);

            uint v1 = (uint)renderer.AddVertexToMesh(p1, Vector3.Zero);
            uint v2 = (uint)renderer.AddVertexToMesh(p2, Vector3.Zero);
            uint v3 = (uint)renderer.AddVertexToMesh(p3, Vector3.Zero);
            uint v4 = (uint)renderer.AddVertexToMesh(p4, Vector3.Zero);
            uint v5 = (uint)renderer.AddVertexToMesh(p5, Vector3.Zero);
            uint v6 = (uint)renderer.AddVertexToMesh(p6, Vector3.Zero);
            uint v7 = (uint)renderer.AddVertexToMesh(p7, Vector3.Zero);
            uint v8 = (uint)renderer.AddVertexToMesh(p8, Vector3.Zero);

            //front
            renderer.AddTriangleToMesh(v1, v2, v3);
            renderer.AddTriangleToMesh(v1, v3, v4);

            //top
            renderer.AddTriangleToMesh(v5, v2, v1);
            renderer.AddTriangleToMesh(v5, v6, v2);

            //right
            renderer.AddTriangleToMesh(v2, v6, v3);
            renderer.AddTriangleToMesh(v6, v7, v3);

            //back
            renderer.AddTriangleToMesh(v5, v6, v8);
            renderer.AddTriangleToMesh(v6, v7, v8);

            //bottom
            renderer.AddTriangleToMesh(v8, v7, v4);
            renderer.AddTriangleToMesh(v7, v3, v4);

            //left
            renderer.AddTriangleToMesh(v1, v5, v8);
            renderer.AddTriangleToMesh(v1, v8, v4);
        }

        public void Render(MeshRenderer renderer)
        {
            renderer.Render();
        }
    }
}
