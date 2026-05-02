using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using System.Windows.Forms;
using YTDownloader.Data;
using YTDownloader.Service;

namespace YTDownloader.Startup;

internal class AppStartup
{
    public IContainer Container { get; private set; } = null!;

    public void Run()
    {
        // 1. Log 必須最先初始化（後續步驟都需要 logger）
        new LogInitializer().Initialize();

        // 2. 載入設定
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .Build();

        // 3. 建立 DI Container
        Container = BuildContainer(config);

        // 4. 依序執行其餘初始化流程
        var initializers = new IAppInitializer[]
        {
            Container.Resolve<DatabaseInitializer>()
            // 之後新增其他初始化步驟直接加在這裡
        };

        foreach (var initializer in initializers)
            initializer.Initialize();
    }

    private static IContainer BuildContainer(IConfiguration config)
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSerilog(dispose: true));

        var assembly = Assembly.GetExecutingAssembly();
        var builder = new ContainerBuilder();

        // 將 Microsoft.Extensions.DependencyInjection 註冊的 logging 等服務轉入 Autofac。
        builder.Populate(services);

        builder.RegisterInstance(config).As<IConfiguration>().SingleInstance();
        builder.RegisterType<DatabaseInitializer>().AsSelf().InstancePerDependency();

        // 依資料夾/型別約定集中註冊 Service 與 UI，避免在這裡逐一手寫每個 class。
        RegisterServices(builder, assembly, config);
        RegisterForms(builder, assembly);

        return builder.Build();
    }

    private static void RegisterServices(ContainerBuilder builder, Assembly assembly, IConfiguration config)
    {
        // 實作 IService 的服務視為應用程式層服務，整個程式生命週期共用同一個實例。
        builder.RegisterAssemblyTypes(assembly)
            .AssignableTo<IService>()
            .AsSelf()
            .As<IService>()
            .SingleInstance();

        // Service namespace 底下未實作 IService 的支援服務也交給 DI 管理。
        builder.RegisterAssemblyTypes(assembly)
            .Where(type =>
                type.Namespace == "YTDownloader.Service" &&
                !typeof(IService).IsAssignableFrom(type) &&
                type != typeof(YtDlpDownloadService))
            .AsSelf()
            .SingleInstance();

        // YtDlpDownloadService 需要從設定組合外部工具路徑，因此用 factory 建立。
        builder.Register(context =>
            {
                var ytDlpPath = ResolveAppPath(config["Path:yt-dlp"]);
                var ffmpegPath = ResolveAppPath(config["Path:ffmpeg"]);
                var ffmpegFolder = File.Exists(ffmpegPath)
                    ? Path.GetDirectoryName(ffmpegPath)
                    : ffmpegPath;

                return new YtDlpDownloadService(
                    ytDlpPath,
                    ffmpegFolder,
                    context.Resolve<Microsoft.Extensions.Logging.ILogger<YtDlpDownloadService>>());
            })
            .AsSelf()
            .SingleInstance();
    }

    private static void RegisterForms(ContainerBuilder builder, Assembly assembly)
    {
        // 一般視窗每次開啟都建立新實例，避免重用已關閉或已 Dispose 的 Form。
        builder.RegisterAssemblyTypes(assembly)
            .AssignableTo<Form>()
            .Where(type => type != typeof(MainForm))
            .AsSelf()
            .InstancePerDependency();

        // 主視窗是應用程式根 UI，生命週期與程式一致。
        builder.RegisterType<MainForm>()
            .AsSelf()
            .SingleInstance();
    }

    private static string ResolveAppPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        // appsettings.json 允許寫絕對路徑或相對於目前工作目錄的路徑。
        return Path.IsPathRooted(relativePath)
            ? relativePath.Trim()
            : Path.Combine(Environment.CurrentDirectory, relativePath.Trim());
    }
}
