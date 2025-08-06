using System;
using System.IO;

namespace WpfDwmManager.Utils
{
    /// <summary>
    /// Simple logging utility for the DWM Manager
    /// </summary>
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WpfDwmManager",
            "logs",
            $"dwm-{DateTime.Now:yyyy-MM-dd}.log");

        private static readonly object LogLock = new object();

        static Logger()
        {
            // Ensure log directory exists
            var logDir = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Log("WARN", message);
        }

        public static void LogError(string message, Exception? exception = null)
        {
            var fullMessage = exception != null ? $"{message}: {exception}" : message;
            Log("ERROR", fullMessage);
        }

        public static void LogDebug(string message)
        {
            var config = ConfigManager.LoadConfiguration();
            if (config.LogLevel.Equals("Debug", StringComparison.OrdinalIgnoreCase))
            {
                Log("DEBUG", message);
            }
        }

        private static void Log(string level, string message)
        {
            try
            {
                lock (LogLock)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                    
                    // Write to console for immediate feedback
                    Console.WriteLine(logEntry);
                    
                    // Also write to log file
                    File.AppendAllText(LogPath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Don't throw exceptions from logging - just fail silently
                // to avoid breaking the main application
            }
        }

        public static void LogWindowOperation(string operation, IntPtr windowHandle, string windowTitle = "")
        {
            var title = !string.IsNullOrEmpty(windowTitle) ? $" ({windowTitle})" : "";
            LogDebug($"Window Operation: {operation} - Handle: {windowHandle}{title}");
        }

        public static void LogLayoutChange(string fromLayout, string toLayout, int windowCount)
        {
            LogInfo($"Layout changed from {fromLayout} to {toLayout} with {windowCount} windows");
        }

        public static void LogHotKeyAction(string hotkey, string action)
        {
            LogDebug($"Hotkey activated: {hotkey} -> {action}");
        }

        public static string GetLogPath()
        {
            return LogPath;
        }

        public static void ClearOldLogs(int daysToKeep = 7)
        {
            try
            {
                var logDir = Path.GetDirectoryName(LogPath);
                if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(logDir, "dwm-*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        fileInfo.Delete();
                        LogInfo($"Deleted old log file: {fileInfo.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to clear old logs", ex);
            }
        }
    }
}