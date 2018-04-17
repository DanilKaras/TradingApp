using TradingApp.Data.Enums;

namespace TradingApp.Data.Models
{
    public class CoinPerformance
    {
        public Indicator Indicator { get; set; }
        public decimal Rate { get; set; }
    }
}