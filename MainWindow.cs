using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Voxel
{
    public class MainWindow : GameWindow
    {
        ChunkManager chunkManager;

        Camera camera;
        MovementController movementController;

        int frames = 0;
        double framesTime = 0;

        bool paused = false;

        public MainWindow(int width, int height, string windowTitle) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = windowTitle })
        {
            movementController = new MovementController(this);
            camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f), 1.0f);
            chunkManager = new ChunkManager();
        }

        bool doRenderUpdates = false;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);     

            KeyboardState input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (input.IsKeyPressed(Keys.P))
            {
                if (!paused)
                {
                    paused = true;
                    CursorState = CursorState.Normal;
                    this.MousePosition = new Vector2(this.Size.X / 2f, this.Size.Y / 2f);
                }
                else
                {
                    paused = false;
                    movementController.FirstMove = true;
                    CursorState = CursorState.Grabbed;
                }
            }

            if (!paused)
            {
                movementController.HandleInput(camera, e);
            }

            chunkManager.Update(camera);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            CursorState = CursorState.Grabbed;

            GL.ClearColor(0f, 0f, 0f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            chunkManager.Render(this);

            frames++;
            framesTime += args.Time;

            if (framesTime >= 1)
            {
                Console.WriteLine("FPS: " + frames);
                frames = 0;
                framesTime = 0;
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);

            camera.AspectRatio = (float)e.Width / e.Height;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }
    }
}
