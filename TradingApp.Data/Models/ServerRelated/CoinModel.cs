using System.Collections.Generic;

namespace TradingApp.Data.Models
{
    public class CoinModel
    {
        public string Response { get; set; }

        public string Type { get; set; }

        public string Aggregated { get; set; }
        public string TimeTo { get; set; }
        public string TimeFrom { get; set; }
        public string FirstValueInArray { get; set; }
        public Conversion Conversion {get;set;}
        public List<CoinData> Data { get; set; }
    }
}