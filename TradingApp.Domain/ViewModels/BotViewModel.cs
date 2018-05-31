using System.Collections.Generic;
using TradingApp.Domain.Models;

namespace TradingApp.Domain.ViewModels
{
    public class BotViewModel
    {
        public List<string> Buy { get; set; }
        public List<string> Consider { get; set; }
        public List<string> DontBuy { get; set; }
        public List<ExcelBotArrangeLog> Report { get; set; }
        public string CallsMadeHisto { get; set; }
        public string CallsLeftHisto { get; set; }
    }
}