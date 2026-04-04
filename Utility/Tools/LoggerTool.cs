using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Utility.Tools
{
    public static class LoggerTool
    {
        public static ILogger GetLogger(string catalog, bool consoleOutput = true)
        {

            Log.Logger = GetSeriLog(consoleOutput);
            var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSerilog());

            return loggerFactory.CreateLogger(catalog);
        }

        public static Logger GetSeriLog(bool consoleOutput)
        {
            if (Log.Logger is Logger logger)
            {
                if (logger != Logger.None)
                    return logger;
            }

            var configuration = ParameterTool.GetConfiguration();

            if (configuration != null)
            {
                var cfg = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration);
                if (consoleOutput)
                    cfg = cfg.WriteTo.Console();

                return cfg.CreateLogger();
            }
            else
            {
                return new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.Console()
                     .CreateLogger();
            }

        }
    }
}
