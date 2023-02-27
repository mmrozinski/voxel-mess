using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGL
{
    public class MeshRenderer
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();

        int VertexArrayObject;
        int VertexBufferObject;
        int ElementBufferObject;

        Matrix4 model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
        Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 800 / 600, 0.1f, 100.0f);

        Shader shader;
        bool usesExternalShader;

        /// <summary>
        /// Creates a new MeshRenderer with the default shaders
        /// </summary>
        public MeshRenderer()
        {
            shader = new Shader("shader.vert", "shader.frag"); // TODO: Implement switching to alternative shaders

            GL.Enable(EnableCap.DepthTest);

            shader.Use();

            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            ElementBufferObject = GL.GenBuffer();

        }
        
        /// <summary>
        /// Creates a new MeshRenderer using the specified shader
        /// </summary>
        /// <param name="shader"></param>
        public MeshRenderer(Shader shader)
        {
            this.shader = shader;
            usesExternalShader = true;

            GL.Enable(EnableCap.DepthTest);

            VertexArrayObject = GL.GenVertexArray();
            VertexBufferObject = GL.GenBuffer();
            ElementBufferObject = GL.GenBuffer();

        }

        /// <summary>
        /// Adds a new vertex to the rendered mesh
        /// </summary>
        /// <param name="position">vertex's coordinates</param>
        /// <returns>the inserted vertex's id</returns>
        public int AddVertexToMesh(Vector3 position, Vector3 normal)
        {
            vertices.Add(position.X);
            vertices.Add(position.Y);
            vertices.Add(position.Z);

            vertices.Add(normal.X);
            vertices.Add(normal.Y);
            vertices.Add(normal.Z);

            return (vertices.Count / 6) - 1;
        }

        /// <summary>
        /// Adds a triangle to the indices list
        /// </summary>
        /// <param name="v1">vertex 1</param>
        /// <param name="v2">vertex 2</param>
        /// <param name="v3">vertex 3</param>
        public void AddTriangleToMesh(uint v1, uint v2, uint v3)
        {
            indices.Add(v1);
            indices.Add(v2);
            indices.Add(v3);
        }

        public void SetProjectionMatrix(Matrix4 matrix)
        {
            projection = matrix;
        }

        public void SetViewMatrix(Matrix4 matrix)
        {
            view = matrix;
        }

        public void SetModelMatrix(Matrix4 matrix)
        {
            model = matrix;
        }

        public bool IsEmpty()
        {
            return vertices.Count == 0;
        }

        public void Clear()
        {
            vertices.Clear();
            indices.Clear();
        }

        public void Render()
        {
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(VertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aPosition"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(shader.GetAttribLocation("aNormal"));
            GL.VertexAttribPointer(shader.GetAttribLocation("aNormal"), 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
        }

        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!usesExternalShader)
                    shader.Dispose();

                disposedValue = true;
            }
        }

        ~MeshRenderer()
        {
            if (!usesExternalShader)
                shader.Dispose();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
