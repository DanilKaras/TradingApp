using System.Collections.Generic;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;

namespace TradingApp.Domain.ViewModels
{
    public class AutoComponentsViewModel
    {
        public IEnumerable<TableRow> Table { get; set; }
        public string ComponentsPath { get; set; }
        public string ForecastPath { get; set; }
        public string AssetName { get; set; }
        public Indicator Indicator { get; set; }
    }
}