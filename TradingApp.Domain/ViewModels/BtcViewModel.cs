﻿using System.Collections.Generic;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;

namespace TradingApp.Domain.ViewModels
{
    public class BtcViewModel
    {
        public string Rate { get; set; }
        public string ForecastPath { get; set; }
        public string AssetName { get; set; }
        public Indicator Indicator { get; set; }
        public string Volume { get; set; }
        public string Change { get; set; }
        public string CallsMadeHisto { get; set; }
        public string CallsLeftHisto { get; set; }
        public string Rsi { get; set; }
        public string Width { get; set; }
    }
}