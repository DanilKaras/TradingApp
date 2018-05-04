using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.PortableExecutable;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Core.Core
{
    public static class Shared
    {
        private static readonly object _locker;
        private static readonly List<ExcelLog> _log;
         
        public static IEnumerable<ExcelLog> GetLog => _log;
        static Shared()
        {
           _locker = new object(); 
           _log = new List<ExcelLog>();  
        }
        
        public static void Log(string assetName, Indicator result, decimal rate, decimal volume, decimal change, decimal rsi)
        {
            lock (_locker)
            {
                _log.Add(new ExcelLog()
                {
                    AssetName = assetName,
                    Log = result.ToString(),
                    Rate = rate.ToString(CultureInfo.InvariantCulture),
                    Volume = volume.ToString(CultureInfo.InvariantCulture) + " BTC",
                    Change = change.ToString("N2"),
                    Rsi = rsi.ToString("N2") + "%"
                });             
            }
        }
        
        public static void ClearLog()
        {
            lock (_locker)
            {
                _log.Clear();
            }
        }
        
        public static void WrirteTest(string path, List<CoinOptimized> coin)
        {
            var fileDestination = Path.Combine(path, "test.xlsx");
            var file = new FileInfo(fileDestination);
            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                var rowNumber = 1;
                foreach (var subLog in coin)
                {
                    worksheet.Cells[rowNumber, 1].Value = subLog.Close;
                    rowNumber++;
                }
                package.Save();
            }
        }
    }
}