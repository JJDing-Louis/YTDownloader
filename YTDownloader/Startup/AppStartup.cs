using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YTDownloader.Data;
using YTDownloader.Service;

namespace YTDownloader.Startup
{
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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // 3. 建立 DI Container
            Container = BuildContainer(config);

            // 4. 依序執行其餘初始化流程
            var initializers = new IAppInitializer[]
            {
                Container.Resolve<DatabaseInitializer>(),
                // 之後新增其他初始化步驟直接加在這裡
            };

            foreach (var initializer in initializers)
                initializer.Initialize();
        }

        private static IContainer BuildContainer(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddLogging(b => b.AddSerilog(dispose: true));

            var builder = new ContainerBuilder();
            builder.Populate(services);

            builder.RegisterInstance(config).As<IConfiguration>().SingleInstance();
            builder.RegisterType<DatabaseInitializer>().AsSelf().InstancePerDependency();
            builder.RegisterType<MainInitializationService>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
