using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Voxel
{
    public class MovementController
    {
        NativeWindow window;

        float speed = 10.0f;
        float sensitivity = 0.2f;
        Vector2 lastPos;
        public bool FirstMove { private get; set; } = true;

        public MovementController(NativeWindow window)
        {
            this.window = window;
        }

        public void HandleInput(Camera camera, FrameEventArgs e)
        {
            //Keyboard
            KeyboardState input = window.KeyboardState;

            if (input.IsKeyDown(Keys.W))
            {
                camera.Position += camera.Front * speed * (float)e.Time; //Forward 
            }

            if (input.IsKeyDown(Keys.S))
            {
                camera.Position -= camera.Front * speed * (float)e.Time; //Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed * (float)e.Time; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * speed * (float)e.Time; //Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                camera.Position += camera.Up * speed * (float)e.Time; //Up 
            }

            if (input.IsKeyDown(Keys.LeftShift))
            {
                camera.Position -= camera.Up * speed * (float)e.Time; //Down
            }

            //Mouse
            MouseState mouse = window.MouseState;

            if (FirstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                FirstMove = false;
            }
            else
            {
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }
            
        }
    }
}
