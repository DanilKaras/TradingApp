using System.ComponentModel;

namespace TradingApp.Data.Models
{
    public class CallsLeft
    {
        [DisplayName("Calls Left:")]
        public string Histo { get; set; }
        public string Price { get; set; }
        public string News { get; set; }
        public string Strict { get; set; }
    }
}