using JJNET.Utility.Tools;
using Serilog;

namespace YTDownloader.Startup;

internal class LogInitializer : IAppInitializer
{
    public void Initialize()
    {
        Log.Logger = LoggerTool.GetSeriLog(true);
        Log.Information("Logger initialized.");
    }
}