using System.Collections.Generic;

namespace TradingApp.Web.Models
{
    public class ManualViewModel
    {
        //public IEnumerable<TableRow> Table { get; set; }
        public string ComponentsPath { get; set; }
        public string ForecastPath { get; set; }
        public string AssetName { get; set; }
        public int RequestsPerDay { get; set; }
        //public Indicator Indicator { get; set; }
        //public List<ExcelLog> Report { get; set; }
    }
}