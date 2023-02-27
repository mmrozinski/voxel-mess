using OpenTK.Mathematics;

namespace Voxel
{
    public class Chunk
    {
        public const int CHUNK_SIZE = 8;
        public enum Sides
        {
            NORTH = 0, SOUTH, WEST, EAST, TOP, BOTTOM,
        }
        public Block[,,] Blocks { get; private set; } = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

        public Vector3i Position { get; private set; }

        private bool isInitialized = false;
        private bool isGenerated = false;

        public bool IsLoaded { get; private set; }
        public bool IsSetup { get; private set; }
        public bool ShouldRender { get; set; }
        public bool IsEmpty { get; private set; }
        public bool NeedsRebuild { get; private set; } = true;

        public HashSet<Sides> FullSides = new HashSet<Sides>();

        private int? meshId = null;

        /// <summary>
        /// Converts the global float coordinates to global chunk grid int coordinates
        /// </summary>
        /// <param name="position">global position</param>
        /// <returns>chunk grid position</returns>
        public static Vector3i VoxelToChunkPosition(Vector3 position)
        {
            Vector3 vec = ((position) / CHUNK_SIZE / Block.BLOCK_RENDER_SIZE);
            int x = (int)Math.Floor(vec.X);
            int y = (int)Math.Floor(vec.Y);
            int z = (int)Math.Floor(vec.Z);

            return new Vector3i(x, y, z);
        }

        public static Vector3 ChunkToVoxelPosition(Vector3i position)
        {
            return new Vector3((Vector3)(position) * CHUNK_SIZE * Block.BLOCK_RENDER_SIZE);
        }

        public Chunk(Vector3i position)
        {
            Position = position;
        }

        private void Initialize()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        Blocks[x, y, z] = new Block();
                    }
                }
            }

            isInitialized = true;
        }

        private void Generate(FastNoiseLite noise)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Vector3 globalPosition = ChunkToVoxelPosition(Position);

                    float height = noise.GetNoise(globalPosition.X + x, globalPosition.Z + z) * CHUNK_SIZE * 8;

                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        if (globalPosition.Y + y > height)
                            break;
                        Blocks[x, y, z].Active = true;
                    }
                }
            }
        }

        private void UpdateFullSides()
        {
            FullSides.Clear();

            foreach (var side in Enum.GetValues(typeof(Sides)).Cast<Sides>())
            {
                FullSides.Add(side);
            }

            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    if (!Blocks[x, y, 0].Active)
                        FullSides.Remove(Sides.SOUTH);
                    if (!Blocks[x, y, CHUNK_SIZE - 1].Active)
                        FullSides.Remove(Sides.NORTH);
                }

                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (!Blocks[x, 0, z].Active)
                        FullSides.Remove(Sides.BOTTOM);
                    if (!Blocks[x, CHUNK_SIZE - 1, z].Active)
                        FullSides.Remove(Sides.TOP);
                }
            }

            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (!Blocks[0, y, z].Active)
                        FullSides.Remove(Sides.EAST);
                    if (!Blocks[CHUNK_SIZE - 1, y, z].Active)
                        FullSides.Remove(Sides.WEST);
                }
            }
        }

        public void Load()
        {
            if (!isInitialized)
                Initialize();
            IsLoaded = true;
        }

        public void Unload(ChunkRenderer renderer)
        {
            if (meshId is not null)
            {
                renderer.removeChunk((int)meshId);
                meshId = null;
            }

            IsLoaded = false;
            IsSetup = false;
            ShouldRender = false;
        }

        public void Setup(ChunkRenderer renderer, FastNoiseLite noise)
        {
            if (!isGenerated)
                Generate(noise);
            meshId = renderer.AddChunk(this);
            UpdateFullSides();
            IsSetup = true;
        }

        public void RebuildMesh(ChunkRenderer renderer)
        {
            if (meshId is not null)
            {
                renderer.removeChunk((int)meshId);
                meshId = null;
            }
            meshId = renderer.AddChunk(this);
            UpdateFullSides();
            NeedsRebuild = false;
        }

        public void UpdateRenderFlags(ChunkRenderer renderer)
        {
            if (meshId is not null)
                IsEmpty = renderer.IsEmpty((int)meshId);
            else
                IsEmpty = true;

            if (IsEmpty || !IsLoaded || !IsSetup)
                ShouldRender = false;
            else
            {
                if (IsLoaded && IsSetup)
                    ShouldRender = true;
            }
        }

        public void Render(ChunkRenderer renderer)
        {
            if (meshId is not null)
                renderer.Render((int)meshId);
        }
    }
}
