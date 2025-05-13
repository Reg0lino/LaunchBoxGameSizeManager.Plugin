// --- START OF FILE Utils/FileLogger.cs ---
using System;
using System.IO;
using System.Reflection; // Required for Assembly to get plugin path

namespace LaunchBoxGameSizeManager.Utils
{
    public static class FileLogger
    {
        private static string _logFilePath = null;
        private static readonly object _lock = new object();

        // Call this ONCE, very early, e.g., in plugin constructor
        public static void Initialize(string pluginName)
        {
            try
            {
                string pluginDirectory;
                try
                {
                    // Get the directory where the plugin DLL is running
                    pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                catch
                {
                    // Fallback if GetExecutingAssembly().Location fails (e.g., in some restricted environments)
                    pluginDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaunchBox", "Plugins", pluginName, "Logs");
                    Directory.CreateDirectory(pluginDirectory); // Ensure fallback log directory exists
                }

                if (string.IsNullOrEmpty(pluginDirectory)) // Final fallback
                {
                    pluginDirectory = Path.GetTempPath(); // Should always be writable
                }

                _logFilePath = Path.Combine(pluginDirectory, $"{pluginName}_debug_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                Log("FileLogger Initialized. Logging to: " + _logFilePath);
            }
            catch (Exception ex)
            {
                // If logger init fails, we can't log. Console.WriteLine might be visible if debugger attached early enough.
                Console.WriteLine($"CRITICAL: FileLogger initialization failed: {ex.Message}");
                _logFilePath = null;
            }
        }

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                Console.WriteLine($"FileLogger not initialized. Message: {message}");
                return; // Logger not initialized
            }

            try
            {
                lock (_lock) // Basic thread safety
                {
                    File.AppendAllText(_logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                // Silently ignore logging errors to prevent crashing the plugin
                Console.WriteLine($"FileLogger Error writing log: {ex.Message}. Original Message: {message}");
            }
        }

        public static void LogError(string message, Exception ex = null)
        {
            string fullMessage = $"ERROR: {message}";
            if (ex != null)
            {
                fullMessage += $"{Environment.NewLine}Exception: {ex.ToString()}"; // Includes stack trace
            }
            Log(fullMessage);
        }
    }
}
// --- END OF FILE Utils/FileLogger.cs ---