using System.Runtime.CompilerServices;
using Autofac;
using YTDownloader.Startup;

[assembly: InternalsVisibleTo("YTDownloaderTest")]

namespace YTDownloader;

internal static class Program
{
    public static AppStartup Startup { get; } = new();

    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Startup.Run();
        Application.Run(Startup.Container.Resolve<MainForm>());
    }
}