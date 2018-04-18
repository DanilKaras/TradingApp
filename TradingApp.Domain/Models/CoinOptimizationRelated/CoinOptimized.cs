using System;

namespace TradingApp.Domain.Models.CoinOptimizationRelated
{
    public class CoinOptimized
    {
        public DateTime Time { get; set; }

        public string Close { get; set; }

        public string High { get; set; }

        public string Low { get; set; }

        public string Open { get; set; }

        public string VolumeFrom { get; set; }

        public string VolumeTo { get; set; }
    }
}