using System.Collections.Generic;
using TradingApp.Data.Enums;
using TradingApp.Domain.Models;

namespace TradingApp.Domain.ViewModels
{
    public class ManualViewModel
    {
        public IEnumerable<TableRow> Table { get; set; }
        public string ComponentsPath { get; set; }
        public string ForecastPath { get; set; }
        public string AssetName { get; set; }
        public Indicator Indicator { get; set; }
        public string CallsMadeHisto { get; set; }
        public string CallsLeftHisto { get; set; }
    }
}