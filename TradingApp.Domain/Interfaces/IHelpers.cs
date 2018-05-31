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
        BotViewModel GetLatestArranged();
        IEnumerable<string> GetAssets();
        AutoComponentsViewModel GetForecastData(Indicator indicator, string assetName, int periods);
        ArrangeBotComponentViewModel GetBotArrangedForecastData(BotArrange arrange, string assetName, int periods);
        void WriteObservables(IEnumerable<string> assets);
        void WritAsstesForBot(IEnumerable<string> assets);
    }
}