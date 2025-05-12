using System;

namespace LaunchBoxGameSizeManager.Utils
{
    public static class SizeTierGenerator
    {
        private const long KB = 1024;
        private const long MB = KB * 1024;
        private const long GB = MB * 1024;

        // This method now assumes sizeInBytes is >= 0 for actual tiering
        // Special values like DO_NOT_STORE_SIZE_CODE from LaunchBoxDataService won't be passed here for tiering.
        public static string GetSizeTier(long sizeInBytes)
        {
            if (sizeInBytes < 0)
            {
                // This case should ideally be handled before calling GetSizeTier,
                // meaning only non-negative values are passed for actual tier generation.
                // If it does get a negative, it means an issue upstream.
                return string.Empty; // Or "Unknown Tier" but empty is cleaner if not storing.
            }

            // Check from largest to smallest
            if (sizeInBytes >= 200 * GB) return "01) > 200 GB";
            if (sizeInBytes >= 150 * GB) return "02) 150 GB - 200 GB";
            if (sizeInBytes >= 140 * GB) return "03) 140 GB - 150 GB";
            if (sizeInBytes >= 130 * GB) return "04) 130 GB - 140 GB";
            if (sizeInBytes >= 120 * GB) return "05) 120 GB - 130 GB";
            if (sizeInBytes >= 100 * GB) return "06) 100 GB - 120 GB";
            if (sizeInBytes >= 90 * GB) return "07) 90 GB - 100 GB";
            if (sizeInBytes >= 80 * GB) return "08) 80 GB - 90 GB";
            if (sizeInBytes >= 70 * GB) return "09) 70 GB - 80 GB";
            if (sizeInBytes >= 60 * GB) return "10) 60 GB - 70 GB";
            if (sizeInBytes >= 50 * GB) return "11) 50 GB - 60 GB";
            if (sizeInBytes >= 40 * GB) return "12) 40 GB - 50 GB";
            if (sizeInBytes >= 30 * GB) return "13) 30 GB - 40 GB";
            if (sizeInBytes >= 25 * GB) return "14) 25 GB - 30 GB";
            if (sizeInBytes >= 20 * GB) return "15) 20 GB - 25 GB";
            if (sizeInBytes >= 15 * GB) return "16) 15 GB - 20 GB";
            if (sizeInBytes >= 10 * GB) return "17) 10 GB - 15 GB";
            if (sizeInBytes >= 5 * GB) return "18) 5 GB - 10 GB";
            if (sizeInBytes >= 2 * GB) return "19) 2 GB - 5 GB";
            if (sizeInBytes >= 1 * GB) return "20) 1 GB - 2 GB";
            if (sizeInBytes >= 750 * MB) return "21) 750 MB - 1 GB";
            if (sizeInBytes >= 500 * MB) return "22) 500 MB - 750 MB";
            if (sizeInBytes >= 300 * MB) return "23) 300 MB - 500 MB";
            if (sizeInBytes >= 100 * MB) return "24) 100 MB - 300 MB";
            if (sizeInBytes >= 50 * MB) return "25) 50 MB - 100 MB";
            if (sizeInBytes >= 10 * MB) return "26) 10 MB - 50 MB"; // New Split
            if (sizeInBytes >= 1 * MB) return "27) 1 MB - 10 MB";  // New Split
            if (sizeInBytes >= 100 * KB) return "28) 100 KB - 1 MB";
            if (sizeInBytes >= 50 * KB) return "29) 50 KB - 100 KB";
            if (sizeInBytes >= 5 * KB) return "30) 5 KB - 50 KB";

            return "31) < 5 KB";
        }
    }
}