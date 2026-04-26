using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
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
            var logFilePath = Path.Combine("logs", $"{DateTime.Now:yyyyMMdd}_sys.log");

            if (configuration != null)
            {
                var cfg = ApplyLogLevelSettings(new LoggerConfiguration(), configuration)
                    .WriteTo.File(
                        logFilePath,
                        shared: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");

                if (consoleOutput)
                    cfg = cfg.WriteTo.Console();

                return cfg.CreateLogger();
            }
            else
            {
                return new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.File(
                         logFilePath,
                         shared: true,
                         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                     .WriteTo.Console()
                     .CreateLogger();
            }

        }

        private static LoggerConfiguration ApplyLogLevelSettings(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var defaultLevel = ParseLogEventLevel(configuration["Logging:LogLevel:Default"], LogEventLevel.Information);
            loggerConfiguration = SetMinimumLevel(loggerConfiguration, defaultLevel);

            foreach (var section in configuration.GetSection("Logging:LogLevel").GetChildren())
            {
                if (string.Equals(section.Key, "Default", StringComparison.OrdinalIgnoreCase))
                    continue;

                loggerConfiguration = loggerConfiguration.MinimumLevel.Override(
                    section.Key,
                    ParseLogEventLevel(section.Value, defaultLevel));
            }

            return loggerConfiguration;
        }

        private static LoggerConfiguration SetMinimumLevel(LoggerConfiguration loggerConfiguration, LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Verbose => loggerConfiguration.MinimumLevel.Verbose(),
                LogEventLevel.Debug => loggerConfiguration.MinimumLevel.Debug(),
                LogEventLevel.Information => loggerConfiguration.MinimumLevel.Information(),
                LogEventLevel.Warning => loggerConfiguration.MinimumLevel.Warning(),
                LogEventLevel.Error => loggerConfiguration.MinimumLevel.Error(),
                LogEventLevel.Fatal => loggerConfiguration.MinimumLevel.Fatal(),
                _ => loggerConfiguration.MinimumLevel.Information()
            };
        }

        private static LogEventLevel ParseLogEventLevel(string? level, LogEventLevel defaultLevel)
        {
            return level?.Trim().ToUpperInvariant() switch
            {
                "TRACE" => LogEventLevel.Verbose,
                "VERBOSE" => LogEventLevel.Verbose,
                "DEBUG" => LogEventLevel.Debug,
                "INFORMATION" => LogEventLevel.Information,
                "INFO" => LogEventLevel.Information,
                "WARNING" => LogEventLevel.Warning,
                "WARN" => LogEventLevel.Warning,
                "ERROR" => LogEventLevel.Error,
                "CRITICAL" => LogEventLevel.Fatal,
                "FATAL" => LogEventLevel.Fatal,
                _ => defaultLevel
            };
        }
    }
}
