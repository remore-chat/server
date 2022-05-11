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

        private static void Log(string message, LogLevel level)
        {
            lock (logLock)
            {
                if (level == LogLevel.Info)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (level == LogLevel.Warn)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (level == LogLevel.Err)
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write($"[{level.ToString().ToUpperInvariant()}] ");
                Console.ResetColor();
                Console.Write(message + "\n");
            }
        }
    }
}
