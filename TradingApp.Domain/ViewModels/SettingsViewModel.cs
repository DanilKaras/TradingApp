using System.Collections.Generic;

namespace TradingApp.Domain.ViewModels
{
    public class SettingsViewModel
    {
        public string UpperBorder { get; set; }
        public string LowerBorder { get; set; }
        public string LastExchange { get; set; }
        public string Btc { get; set; }
        public List<string> Exchanges { get; set; }
    }
}