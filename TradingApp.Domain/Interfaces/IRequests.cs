using System.Collections.Generic;
using TradingApp.Domain.Models.CoinOptimizationRelated;
using TradingApp.Domain.Models.ServerRelated;

namespace TradingApp.Domain.Interfaces
{
    public interface IRequests
    {
        ExchangeData GetAssets(string exhangeName);
        List<string> GetExchanges();
        ServerRequestsStats GetStats();
        CoinModel GetCoinData(string symbol);
    }
}