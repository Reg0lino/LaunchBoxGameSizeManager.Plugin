namespace LaunchBoxGameSizeManager.Utils
{
    public static class Constants
    {
        public const string PluginName = "Game Size Manager";
        public const string CustomFieldCalculatedSize = "CalculatedFileSize"; // Stores size in bytes (long)
        public const string CustomFieldCalculatedSizeFormatted = "CalculatedFileSizeFormatted"; // Stores user-friendly string (e.g., "1.2 GB")
        public const string CustomFieldLastScanned = "FileSizeLastScanned"; // Stores DateTime string of last scan
    }
}