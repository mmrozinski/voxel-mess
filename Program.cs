namespace Voxel
{
    internal class Program
    {
        public static int Main()
        {
            using (MainWindow mw = new MainWindow(800, 600, "Voxel"))
            {
                mw.Run();
            }

            return 0;
        }
    }
}
