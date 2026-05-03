using System;
using System.IO;
using System.Threading;

namespace charmap
{
    public static class AppLogger
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "charmap", "logs");

        private static readonly string LogFilePath = Path.Combine(LogDirectory, $"charmap_{DateTime.Now:yyyyMMdd}.log");
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        static AppLogger()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppLogger init failed: {ex.Message}");
            }
        }

        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
            WriteLog("ERROR", fullMessage);
        }

        public static void Fatal(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
            WriteLog("FATAL", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                _semaphore.Wait();
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppLogger write failed: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static string GetLogPath() => LogFilePath;
    }
}
