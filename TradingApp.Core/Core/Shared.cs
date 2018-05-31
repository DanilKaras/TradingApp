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
        private static readonly List<ExcelBotArrangeLog> _botLog;
        public static IEnumerable<ExcelLog> GetLog => _log;
        public static IEnumerable<ExcelBotArrangeLog> GetArrangedBotLog => _botLog;
        static Shared()
        {
           _locker = new object(); 
           _log = new List<ExcelLog>();  
           _botLog = new List<ExcelBotArrangeLog>();
        }
        
        public static void Log(ExcelLog log)
        {
            lock (_locker)
            {
                _log.Add(log);             
            }
        }
        
        public static void ArrangeBotLog(ExcelBotArrangeLog log)
        {
            lock (_locker)
            {
                _botLog.Add(log);             
            }
        }
        
        public static void ClearLog()
        {
            lock (_locker)
            {
                _log.Clear();
            }
        }
        
        public static void ClearArrangeBotLog()
        {
            lock (_locker)
            {
                _botLog.Clear();
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