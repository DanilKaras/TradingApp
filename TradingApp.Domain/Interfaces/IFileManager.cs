using System.Collections.Generic;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Domain.Interfaces
{
    public interface IFileManager
    {
        void WriteAssetsToExcel(string path, ExchangeData exchangeData);
        IEnumerable<string> ReadAssetsFromExcel(string path);
        List<ExcelLog> ReadLog(string path);
        CustomSettings ReadCustomSettings(string json);
        string ConvertCustomSettings(CustomSettings settings);
        bool CreateDataCsv(List<CoinOptimized> model, string saveToLocation);
        OutStats BuildOutTableRows(string path, int period);
        void WriteLogExcel(string path, IEnumerable<ExcelLog> log);
        void WriteObservables(IEnumerable<string> list, string path);
    }
}