using System.Collections.Generic;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Domain.Interfaces
{
    public interface IProcessModel
    {
        List<CoinOptimized> GetDataManual(string symbol, int dataHours);
        List<CoinOptimized> GetDataAuto(string symbol, int dataHours);
    }
}