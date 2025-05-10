using System;

namespace LaunchBoxGameSizeManager.Utils
{
    public static class FormatHelpers
    {
        public static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "N/A";
            if (bytes == 0) return "0 B";

            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{num} {suf[place]}";
        }
    }
}