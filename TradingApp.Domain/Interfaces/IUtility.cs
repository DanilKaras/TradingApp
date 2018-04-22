using TradingApp.Domain.Models;

namespace TradingApp.Domain.Interfaces
{
    public interface IUtility
    {
        CoinPerformance DefinePerformance(OutStats table);
    }
}