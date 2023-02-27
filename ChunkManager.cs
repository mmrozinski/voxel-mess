using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using VoxelGL;

namespace Voxel
{
    public class ChunkManager
    {
        const int CHUNKS_ASYNC_LIMIT_PER_FRAME = 8;
        const int CHUNK_VISIBILITY_DISTANCE = 4;

        const int WORLD_SIZE = 16;
        List<Chunk> masterList = new List<Chunk>(); // HACK: This is a temporary solution, ideally the chunks would get pulled from a save file

        List<Chunk> loadList = new List<Chunk>();
        List<Chunk> setupList = new List<Chunk>();
        List<Chunk> rebuildList = new List<Chunk>();
        List<Chunk> flagsUpdateList = new List<Chunk>();
        List<Chunk> unloadList = new List<Chunk>();
        List<Chunk> visibleList = new List<Chunk>();
        List<Chunk> renderList = new List<Chunk>();

        ChunkRenderer renderer = new ChunkRenderer();
        MeshRenderer meshRenderer = new MeshRenderer();

        FastNoiseLite noise = new FastNoiseLite();

        public ChunkManager()
        {
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetSeed((int)DateTime.Now.Ticks);
        }

        private Chunk? GetChunk(Vector3i position)
        {
            foreach (var chunk in masterList)
            {
                if (chunk.Position == position)
                    return chunk;
            }
            return null;
        }

        /// <summary>
        /// Runs the load process for chunks in the load list
        /// </summary>
        public void UpdateLoadList()
        {
            int chunksLoaded = 0;
            foreach (var chunk in loadList)
            {
                if (chunksLoaded >= CHUNKS_ASYNC_LIMIT_PER_FRAME)
                    break;

                if (!chunk.IsLoaded)
                {
                    chunk.Load();
                    chunksLoaded++;
                }
            }
            loadList.Clear();
        }

        /// <summary>
        /// Runs the setup process for chunks in the setup list
        /// </summary>
        public void UpdateSetupList()
        {
            foreach (var chunk in setupList)
            {
                if (chunk.IsLoaded && !chunk.IsSetup)
                {
                    chunk.Setup(renderer, noise);
                }
            }
            setupList.Clear();
        }

        /// <summary>
        /// Rebuilds the mesh for chunks in the rebuild list. Also marks chunks for flag update
        /// </summary>
        public void UpdateRebuildList()
        {
            int chunksRebuilt = 0;
            foreach (var chunk in rebuildList)
            {
                if (chunk.IsLoaded && chunk.IsSetup)
                {
                    if (chunksRebuilt >= CHUNKS_ASYNC_LIMIT_PER_FRAME)
                        break;

                    chunk.RebuildMesh(renderer);

                    flagsUpdateList.Add(chunk);

                    Chunk? neighbour;

                    neighbour = GetChunk(chunk.Position + new Vector3i(-1, 0, 0));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    neighbour = GetChunk(chunk.Position + new Vector3i(1, 0, 0));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, -1, 0));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 1, 0));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 0, -1));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 0, 1));
                    if (neighbour is not null)
                        flagsUpdateList.Add(neighbour);

                    chunksRebuilt++;
                }
            }
            rebuildList.Clear();
        }

        public void UpdateFlagsList()
        {
            foreach (var chunk in flagsUpdateList)
            {
                chunk.UpdateRenderFlags(renderer);

                if (chunk.ShouldRender)
                {
                    Chunk? neighbour;

                    bool isSurrounded = true;

                    neighbour = GetChunk(chunk.Position + new Vector3i(-1, 0, 0));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.WEST))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    neighbour = GetChunk(chunk.Position + new Vector3i(1, 0, 0));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.EAST))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, -1, 0));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.TOP))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 1, 0));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.BOTTOM))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 0, -1));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.NORTH))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    neighbour = GetChunk(chunk.Position + new Vector3i(0, 0, 1));
                    if (neighbour is not null && isSurrounded)
                    {
                        if (!neighbour.FullSides.Contains(Chunk.Sides.WEST))
                            isSurrounded = false;
                    }
                    else
                        isSurrounded = false;

                    if (isSurrounded)
                        chunk.ShouldRender = false;
                }
            }
            flagsUpdateList.Clear();
        }

        /// <summary>
        /// Unloads chunks from the unload list
        /// </summary>
        public void UpdateUnloadList()
        {
            foreach (var chunk in unloadList)
            {
                if (chunk.IsLoaded)
                {
                    chunk.Unload(renderer);
                }
            }
            unloadList.Clear();
        }

        /// <summary>
        /// Builds the visibility list for the provided position. Also fills all other lists with matching chunks in visible radius
        /// </summary>
        /// <param name="position">current camera position</param>
        public void UpdateVisible(Vector3 position)
        {
            visibleList.Clear();

            for (int x = -CHUNK_VISIBILITY_DISTANCE; x < CHUNK_VISIBILITY_DISTANCE; x++)
            {
                for (int y = -CHUNK_VISIBILITY_DISTANCE; y < CHUNK_VISIBILITY_DISTANCE; y++)
                {
                    for (int z = -CHUNK_VISIBILITY_DISTANCE; z < CHUNK_VISIBILITY_DISTANCE; z++)
                    {
                        Vector3i chunkPosition = Chunk.VoxelToChunkPosition(position) + new Vector3i(x, y, z);
                        Chunk? chunk = GetChunk(chunkPosition);

                        if (chunk is null)
                        {
                            //if (chunkPosition.X >= 0 && chunkPosition.Y >= 0 && chunkPosition.Z >= 0 && chunkPosition.X < WORLD_SIZE && chunkPosition.Y < WORLD_SIZE && chunkPosition.Z < WORLD_SIZE)
                            {
                                chunk = new Chunk(chunkPosition);
                                masterList.Add(chunk);
                            }
                            // HACK: This is a temporary world size limitation, the goal is for the world to be 'infinite'
                        }
                    }
                }
            }

            foreach (var chunk in masterList)
            {
                Vector3i chunkPosition = Chunk.VoxelToChunkPosition(position);
                Vector3i distance = chunk.Position - chunkPosition;

                //Console.WriteLine("Chunk: " + chunkPosition + " ||| Global: " + position);

                int xDist = Math.Abs(distance.X);
                int yDist = Math.Abs(distance.Y);
                int zDist = Math.Abs(distance.Z);

                if (!chunk.IsLoaded && xDist <= CHUNK_VISIBILITY_DISTANCE && yDist <= CHUNK_VISIBILITY_DISTANCE && zDist <= CHUNK_VISIBILITY_DISTANCE)
                    loadList.Add(chunk);

                if (!chunk.IsSetup)
                    setupList.Add(chunk);

                if (chunk.NeedsRebuild)
                    rebuildList.Add(chunk);

                if (xDist > CHUNK_VISIBILITY_DISTANCE || yDist > CHUNK_VISIBILITY_DISTANCE || zDist > CHUNK_VISIBILITY_DISTANCE)
                    unloadList.Add(chunk);

                if (chunk.IsLoaded && chunk.IsSetup && !chunk.ShouldRender)
                    flagsUpdateList.Add(chunk);

                visibleList.Add(chunk);
            }
        }

        Frustum frustum;

        /// <summary>
        /// Builds the render list based on the existing visibility list
        /// </summary>
        public void UpdateRenderList(Camera camera)
        {
            Matrix4 projection = camera.GetProjectionMatrix();
            Matrix4 view = camera.GetViewMatrix();
            frustum = new Frustum(view * projection);

            renderList.Clear();

            foreach (var chunk in visibleList)
            {
                if (chunk.IsLoaded && chunk.IsSetup)
                {
                    if (chunk.ShouldRender)
                    {
                        float offset = (Chunk.CHUNK_SIZE * Block.BLOCK_RENDER_SIZE) / 2f;
                        Vector3 chunkCenter = Chunk.ChunkToVoxelPosition(chunk.Position) + new Vector3(offset);
                        float size = (Chunk.CHUNK_SIZE * Block.BLOCK_RENDER_SIZE) / 2f;
                        if (frustum.CubeInFrustum(chunkCenter, size, size, size) != Frustum.Intersections.FRUSTUM_OUTSIDE)
                        //if (frustum.SphereInFrustum(chunkCenter, size) != Frustum.Intersections.FRUSTUM_OUTSIDE)
                        {
                            renderList.Add(chunk);
                        }
                        //Debug.WriteLine("Chunk: " + chunk.Position + " or " + chunkCenter + " is " + frustum.CubeInFrustum(chunkCenter, size, size, size));
                    }
                }
            }
        }

        public void Update(Camera camera)
        {
            UpdateLoadList();
            UpdateSetupList();
            UpdateRebuildList();
            UpdateFlagsList();
            UpdateUnloadList();
            UpdateVisible(camera.Position);

            //if (cameraLastPosition != camera.Position || cameraLastFacing != camera.Front)
            {
                UpdateRenderList(camera);
            }

            renderer.SetViewMatrix(camera.GetViewMatrix());
            renderer.SetProjectionMatrix(camera.GetProjectionMatrix());
        }

        public void Update(Camera camera, bool doListUpdates)
        {
            if (doListUpdates)
            {
                UpdateLoadList();
                UpdateSetupList();
                UpdateRebuildList();
                UpdateFlagsList();
                UpdateUnloadList();
                UpdateVisible(camera.Position);

                //if (cameraLastPosition != camera.Position || cameraLastFacing != camera.Front)
                {
                    UpdateRenderList(camera);
                }

                frustum.PreRender(meshRenderer);
            }

            renderer.SetViewMatrix(camera.GetViewMatrix());
            renderer.SetProjectionMatrix(camera.GetProjectionMatrix());

            meshRenderer.SetViewMatrix(camera.GetViewMatrix());
            meshRenderer.SetProjectionMatrix(camera.GetProjectionMatrix());
            meshRenderer.SetModelMatrix(Matrix4.CreateTranslation(Vector3.Zero));
        }

        public void Render(GameWindow window)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var chunk in renderList)
            {
                chunk.Render(renderer);
            }
            window.SwapBuffers();
        }

        ~ChunkManager()
        {
            renderer.Dispose();
        }
    }
}
