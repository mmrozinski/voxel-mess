using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using VoxelGL;

namespace Voxel
{
    public class ChunkRenderer
    {
        List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
        List<int> meshIds = new List<int>();

        Matrix4 view;
        Matrix4 projection;

        Shader shader;

        public ChunkRenderer()
        {
            shader = new Shader("shader.vert", "shader.frag");

            shader.Use();
        }

        public void SetViewMatrix(Matrix4 matrix)
        {
            view = matrix;
            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.SetViewMatrix(view);
            }
        }

        public void SetProjectionMatrix(Matrix4 matrix)
        {
            projection = matrix;
            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.SetProjectionMatrix(projection);
            }
        }

        public bool IsEmpty(int meshId)
        {
            int it = meshIds.IndexOf(meshId);
            return meshRenderers[it].IsEmpty();
        }

        public int AddChunk(Chunk chunk)
        {
            MeshRenderer renderer = new MeshRenderer(shader);
            renderer.SetViewMatrix(view);
            renderer.SetProjectionMatrix(projection);
            renderer.SetModelMatrix(Matrix4.CreateTranslation((Vector3)chunk.Position * (Chunk.CHUNK_SIZE) * Block.BLOCK_RENDER_SIZE));

            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    {
                        if (!chunk.Blocks[x, y, z].Active)
                            continue;

                        bool xNegative = true;
                        if (x > 0) xNegative = !chunk.Blocks[x - 1, y, z].Active;

                        bool xPositive = true;
                        if (x < Chunk.CHUNK_SIZE - 1) xPositive = !chunk.Blocks[x + 1, y, z].Active;

                        bool yNegative = true;
                        if (y > 0) yNegative = !chunk.Blocks[x, y - 1, z].Active;

                        bool yPositive = true;
                        if (y < Chunk.CHUNK_SIZE - 1) yPositive = !chunk.Blocks[x, y + 1, z].Active;

                        bool zNegative = true;
                        if (z > 0) zNegative = !chunk.Blocks[x, y, z - 1].Active;

                        bool zPositive = true;
                        if (z < Chunk.CHUNK_SIZE - 1) zPositive = !chunk.Blocks[x, y, z + 1].Active;


                        CreateCube(xNegative, xPositive, yNegative, yPositive, zNegative, zPositive, x, y, z, renderer);
                    }
                }
            }

            meshRenderers.Add(renderer);
            meshIds.Add(meshIds.DefaultIfEmpty().Max() + 1);

            return meshIds.Max();
        }

        public void removeChunk(int id)
        {
            int it = meshIds.IndexOf(id);
            meshIds.Remove(id);

            meshRenderers[it].Dispose();
            meshRenderers.RemoveAt(it);
        }

        private void CreateCube(bool xNegative, bool xPositive, bool yNegative, bool yPositive, bool zNegative, bool zPositive, float x, float y, float z, MeshRenderer renderer)
        {
            Vector3 p1 = new(x, y, z + Block.BLOCK_RENDER_SIZE);
            Vector3 p2 = new(x + Block.BLOCK_RENDER_SIZE, y, z + Block.BLOCK_RENDER_SIZE);
            Vector3 p3 = new(x + Block.BLOCK_RENDER_SIZE, y + Block.BLOCK_RENDER_SIZE, z + Block.BLOCK_RENDER_SIZE);
            Vector3 p4 = new(x, y + Block.BLOCK_RENDER_SIZE, z + Block.BLOCK_RENDER_SIZE);
            Vector3 p5 = new(x + Block.BLOCK_RENDER_SIZE, y, z);
            Vector3 p6 = new(x, y, z);
            Vector3 p7 = new(x, y + Block.BLOCK_RENDER_SIZE, z);
            Vector3 p8 = new(x + Block.BLOCK_RENDER_SIZE, y + Block.BLOCK_RENDER_SIZE, z);

            Vector3 n;

            uint v1;
            uint v2;
            uint v3;
            uint v4;
            uint v5;
            uint v6;
            uint v7;
            uint v8;

            // Front
            n = Vector3.UnitZ;
            if (zPositive)
            {
                v1 = (uint)renderer.AddVertexToMesh(p1, n);
                v2 = (uint)renderer.AddVertexToMesh(p2, n);
                v3 = (uint)renderer.AddVertexToMesh(p3, n);
                v4 = (uint)renderer.AddVertexToMesh(p4, n);
                renderer.AddTriangleToMesh(v1, v2, v3);
                renderer.AddTriangleToMesh(v1, v3, v4);
            }

            // Back
            n = -Vector3.UnitZ;
            if (zNegative)
            {
                v5 = (uint)renderer.AddVertexToMesh(p5, n);
                v6 = (uint)renderer.AddVertexToMesh(p6, n);
                v7 = (uint)renderer.AddVertexToMesh(p7, n);
                v8 = (uint)renderer.AddVertexToMesh(p8, n);
                renderer.AddTriangleToMesh(v5, v6, v7);
                renderer.AddTriangleToMesh(v5, v7, v8);
            }

            // Right
            n = Vector3.UnitX;
            if (xPositive)
            {
                v2 = (uint)renderer.AddVertexToMesh(p2, n);
                v5 = (uint)renderer.AddVertexToMesh(p5, n);
                v8 = (uint)renderer.AddVertexToMesh(p8, n);
                v3 = (uint)renderer.AddVertexToMesh(p3, n);
                renderer.AddTriangleToMesh(v2, v5, v8);
                renderer.AddTriangleToMesh(v2, v8, v3);
            }

            // Left
            n = -Vector3.UnitX;
            if (xNegative)
            {
                v6 = (uint)renderer.AddVertexToMesh(p6, n);
                v1 = (uint)renderer.AddVertexToMesh(p1, n);
                v4 = (uint)renderer.AddVertexToMesh(p4, n);
                v7 = (uint)renderer.AddVertexToMesh(p7, n);
                renderer.AddTriangleToMesh(v6, v1, v4);
                renderer.AddTriangleToMesh(v6, v4, v7);
            }

            // Top
            n = Vector3.UnitY;
            if (yPositive)
            {
                v4 = (uint)renderer.AddVertexToMesh(p4, n);
                v3 = (uint)renderer.AddVertexToMesh(p3, n);
                v8 = (uint)renderer.AddVertexToMesh(p8, n);
                v7 = (uint)renderer.AddVertexToMesh(p7, n);
                renderer.AddTriangleToMesh(v4, v3, v8);
                renderer.AddTriangleToMesh(v4, v8, v7);
            }

            // Bottom
            n = -Vector3.UnitY;
            if (yNegative)
            {
                v6 = (uint)renderer.AddVertexToMesh(p6, n);
                v5 = (uint)renderer.AddVertexToMesh(p5, n);
                v2 = (uint)renderer.AddVertexToMesh(p2, n);
                v1 = (uint)renderer.AddVertexToMesh(p1, n);
                renderer.AddTriangleToMesh(v6, v5, v2);
                renderer.AddTriangleToMesh(v6, v2, v1);
            }
        }

        public void RenderAll(GameWindow window)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.Render();
            }
            window.SwapBuffers();
        }

        public void Render(int meshId)
        {
            int it = meshIds.IndexOf(meshId);
            meshRenderers[it].Render();
        }

        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (MeshRenderer renderer in meshRenderers)
                {
                    renderer.Dispose();
                }

                disposedValue = true;
            }
        }

        ~ChunkRenderer()
        {
            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.Dispose();
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
