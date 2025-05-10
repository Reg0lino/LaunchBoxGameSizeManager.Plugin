using System;

namespace LaunchBoxGameSizeManager.Models
{
    public class GameSizeInfo
    {
        public string GameId { get; set; }
        public string GameTitle { get; set; }
        public string Platform { get; set; }
        public string ApplicationPath { get; set; }
        public long CalculatedSizeInBytes { get; set; }
        public string CalculatedSizeFormatted { get; set; }
        public DateTime LastScanned { get; set; }
    }
}