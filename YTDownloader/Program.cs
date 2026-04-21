using YTDownloader.Startup;
using YTDownloader.Tool;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("YTDownloaderTest")]

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
            DBTool.InitDB();
            //ApplicationConfiguration.Initialize();
            //Startup.Run();
            //Application.Run(new Main());
        }
    }
}
