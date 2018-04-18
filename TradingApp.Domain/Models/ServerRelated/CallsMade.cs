using System.ComponentModel;

namespace TradingApp.Domain.Models.ServerRelated
{
    public class CallsMade
    {
        [DisplayName("Calls Made:")]
        public string Histo { get; set; }
        public string Price { get; set; }
        public string News { get; set; }
        public string Strict { get; set; }
    }
}