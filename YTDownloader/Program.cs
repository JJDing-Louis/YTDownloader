using YTDownloader.Startup;
using YTDownloader.Service;
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
            ApplicationConfiguration.Initialize();
            Startup.Run();
            DBTool.InitDB();

            var settings = new ConfigSettingService().Init();
            Application.Run(new MainForm(settings));
        }
    }
}
