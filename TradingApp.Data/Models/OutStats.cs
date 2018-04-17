using System.Collections.Generic;

namespace TradingApp.Data.Models
{
    public class OutStats
    {
        public IEnumerable<TableRow> Table { get; set; }
        public decimal MaxValue { get; set; }
        public decimal MinValue { get; set; }
    }
}