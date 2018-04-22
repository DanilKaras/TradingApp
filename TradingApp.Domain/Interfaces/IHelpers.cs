using System.Collections.Generic;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Domain.Interfaces
{
    public interface IHelpers
    {
        SettingsViewModel LoadExchanges();
        CustomSettings UpdateExchanges(SettingsViewModel settings);
        AutoViewModel GetLatestAssets();
        IEnumerable<string> GetAssets();
        AutoComponentsViewModel GetForecastData(Indicator indicator, string assetName, int periods);
        void WriteObservables(List<string> observableList);
    }
}