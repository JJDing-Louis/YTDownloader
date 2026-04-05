using YTDownloader.Startup;

namespace YTDownloader
{
    internal static class Program
    {
        public static AppStartup Startup { get; } = new();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Startup.Run();
            Application.Run(new Main());
        }
    }
}
