using System.Threading.Tasks;
using TradingApp.Domain.Models.ServerRelated;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Domain.Interfaces
{
    public interface IForecaster
    {
        ServerRequestsStats GetStats();
        Task<ManualViewModel> MakeManualForecast(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality);
        Task<AutoViewModel> MakeAutoForecast(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality, string readFrom);
        Task<BtcViewModel> InstantForecast();
    }
}