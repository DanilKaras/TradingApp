using System.Collections.Generic;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Domain.Interfaces
{
    public interface IUtility
    {
        CoinPerformance DefinePerformance(OutStats table);
        MarketFeature GetFeatures(List<CoinOptimized> coin, string coinName);
    }
}