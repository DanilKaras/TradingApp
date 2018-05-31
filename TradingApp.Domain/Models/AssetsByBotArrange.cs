using System.Collections.Generic;

namespace TradingApp.Domain.Models
{
    public class AssetsByBotArrange
    {
        public List<string> BuyAssets { get; set; }
        public List<string> ConsiderAssets { get; set; }
        public List<string> DontBuyAssets { get; set; }
    }
}