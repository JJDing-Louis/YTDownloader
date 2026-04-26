using Serilog;
using JJNET.DataAccess;
using JJNET.Utility.Tools;

namespace YTDownloader.Startup
{
    internal class LogInitializer : IAppInitializer
    {
        public void Initialize()
        {
            Log.Logger = LoggerTool.GetSeriLog(consoleOutput: true);
            Log.Information("Logger initialized.");
        }
    }
}
