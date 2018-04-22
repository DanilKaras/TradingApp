using System.Collections.Generic;

namespace TradingApp.Domain.Models
{
    public class AsstesByIndicator
    {
        public List<string> PositiveAssets { get; set; }
        public List<string> NeutralAssets { get; set; }
        public List<string> NegativeAssets { get; set; }
        public List<string> StrongPositiveAssets { get; set; }

    }
}