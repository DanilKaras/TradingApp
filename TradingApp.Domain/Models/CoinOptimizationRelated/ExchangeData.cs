using System.Collections.Generic;

namespace TradingApp.Domain.Models.CoinOptimizationRelated
{
    public class ExchangeData
    {
        public string ExchangeName { get; set; }
        public List<ExchangeCurrency> Pairs { get; set; }
        public string Btc { get; set; }
    }
}