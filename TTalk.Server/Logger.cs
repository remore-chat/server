using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Server
{
    enum LogLevel
    {
        Info,
        Warn,
        Err,
        Crit
    }

    public static class Logger
    {
        private static object logLock = new object();
        public static void LogInfo(string message) => Log(message, LogLevel.Info);
        public static void LogWarn(string message) => Log(message, LogLevel.Warn);
        public static void LogError(string message) => Log(message, LogLevel.Err);
        public static void LogCritical(string message) => Log(message, LogLevel.Crit);

        private static ILogger _logger;
        public static void Init()
        {
            _logger = ServiceContainer.GetService<ILogger<TTalkServer>>();
        }

        private static void Log(string message, LogLevel level)
        {
            if (level == LogLevel.Info)
                _logger.LogInformation(message);
            else if (level == LogLevel.Warn)
                _logger.LogWarning(message);
            else if (level == LogLevel.Err)
                _logger.LogError(message);
            else
                _logger.LogCritical(message);
        }
    }
}
