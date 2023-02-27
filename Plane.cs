using OpenTK.Mathematics;

namespace Voxel
{
    public class Plane
    {
        public float a;
        public float b;
        public float c;
        public float d;

        public Plane(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public Vector3 GetNormal()
        {
            return new Vector3(a, b, c);
        }

        public float GetPointDistance(Vector3 point)
        {
            return (float)((a * point.X + b * point.Y + c * point.Z + d) / Math.Sqrt(a * a + b * b + c * c));
        }
    }
}
