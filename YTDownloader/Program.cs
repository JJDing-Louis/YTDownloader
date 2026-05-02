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

        try
        {
            // MainForm 由 Autofac 建立，建構子需要的 Service 會在這裡被解析並注入。
            Application.Run(Startup.Container.Resolve<MainForm>());
        }
        finally
        {
            // 關閉應用程式時釋放 Autofac 管理的 singleton / IDisposable 物件。
            Startup.Container.Dispose();
        }
    }
}
