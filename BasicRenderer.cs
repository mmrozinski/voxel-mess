using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelGL
{
    public class BasicRenderer : IDisposable
    {
        List<List<float>> verticesList;
        List<List<uint>> indicesList;

        List<int> VertexArrayObjects;
        List<int> VertexBufferObjects;
        List<int> ElementBufferObjects;

        Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));
        Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 800 / 600, 0.1f, 100.0f);

        Shader shader;
        private bool disposedValue;

        public BasicRenderer()
        {
            verticesList = new List<List<float>>();
            indicesList = new List<List<uint>>();
            shader = new Shader("shader.vert", "shader.frag");

            VertexArrayObjects = new List<int>();
            VertexBufferObjects = new List<int>();
            ElementBufferObjects = new List<int>();

            shader.Use();
        }

        /// <summary>
        /// Creates a new mesh using a vertex array with a vertex and element buffers
        /// </summary>
        /// <returns>The id of the created mesh</returns>
        public int CreateMesh()
        {
            VertexArrayObjects.Add(GL.GenVertexArray());
            VertexBufferObjects.Add(GL.GenBuffer());
            ElementBufferObjects.Add(GL.GenBuffer());
            verticesList.Add(new List<float>());
            indicesList.Add(new List<uint>());

            return VertexArrayObjects.Count - 1;
        }

        public int AddVertexToMesh(int meshId, Vector3 position)
        {
            verticesList[meshId].Add(position.X);
            verticesList[meshId].Add(position.Y);
            verticesList[meshId].Add(position.Z);

            return (verticesList[meshId].Count / 3) - 1;
        }

        public void AddTriangleToMesh(int meshId, uint v1, uint v2, uint v3)
        {
            indicesList[meshId].Add(v1);
            indicesList[meshId].Add(v2);
            indicesList[meshId].Add(v3);
        }

        public void SetProjectionMatrix(Matrix4 matrix)
        {
            projection = matrix;
        }

        public void SetViewMatrix(Matrix4 matrix)
        {
            view = matrix;
        } 

        public void Render(GameWindow window, int meshId)
        {
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(VertexArrayObjects[meshId]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObjects[meshId]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObjects[meshId]);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BufferData(BufferTarget.ArrayBuffer, verticesList[meshId].Count * sizeof(float), verticesList[meshId].ToArray(), BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicesList[meshId].Count * sizeof(uint), indicesList[meshId].ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), shader.GetAttribLocation("aPosition"));
            GL.EnableVertexAttribArray(0);

            GL.DrawElements(PrimitiveType.Triangles, indicesList[meshId].Count, DrawElementsType.UnsignedInt, 0);

            window.SwapBuffers();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                shader.Dispose();

                disposedValue = true;
            }
        }

        ~BasicRenderer()
        {
            shader.Dispose();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
