using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;

namespace TradingApp.Domain.Interfaces
{
    public interface IDirectoryManager
    {
        string AsstesUpdateLocation { get; }
        string AsstesLocation { get; }
        string ObservablesLocationUpdate { get; }
        string ObservablesLocation { get; }
        string AssetsForBotLocation { get; }
        string CustomSettings { get; }
        void UpdateCustomSettings(string json);
        string GenerateForecastFolder(string assetId, int period, DirSwitcher switcher, DateTime? context = null);
        void SaveDataFile(string content, string location);
        Task<bool> WaitForFile(string path, int timeout);
        string FilePathOut(string currentForecastDir);
        string FileForecastOut(string currentForecastDir);
        string FileComponentsOut(string currentComponentsDir);
        ImagesPath ImagePath (DirSwitcher switcher, Indicator? indicator = null, string subFolder = null, string fullPath = null);
        ImagesPath ImagePathByArrange (BotArrange arrange, string subFolder = null, string fullPath = null);
        bool SpecifyDirByTrend(Indicator switcher, string path);
        BotArrange SpecifyDirByIndicators(string path, int rsi, List<int> trend, List<int> border, CoinPerformance performance, decimal coinRsi);
        string GetLastFolder(DirSwitcher switcher);
        void WriteLogToExcel(string path, IEnumerable<ExcelLog> log);
        void WriteArrangeBotLogToExcel(string path, IEnumerable<ExcelBotArrangeLog> log);
        AsstesByIndicator GetListByIndicator(string folder);
        List<ExcelLog> GetReport(string folder);
        List<ExcelBotArrangeLog> GetArrangeBotReport(string folder);
        string GetDirByIndicator(string folder, Indicator indicator);
        string GetDirByArrange(string folder, BotArrange arrange);
        string GetForecastFolderByName(string dir, string assetName);
        void RemoveFolder(string path);
        AssetsByBotArrange GetListByBotArrange(string folder);
    }
}