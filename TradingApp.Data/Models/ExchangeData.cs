using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace TradingApp.Data.Models
{
    public class ExchangeData
    {
        public string ExchangeName { get; set; }
        public List<ExchangeCurrency> Pairs { get; set; }
        public string Btc { get; set; }
    }
}