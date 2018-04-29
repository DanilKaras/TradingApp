using System.Collections.Generic;
using System.Globalization;
using System.Reflection.PortableExecutable;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;

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
        
        public static void Log(string assetName, Indicator result, decimal rate, decimal volume, decimal change)
        {
            lock (_locker)
            {
                _log.Add(new ExcelLog()
                {
                    AssetName = assetName,
                    Log = result.ToString(),
                    Rate = rate.ToString(CultureInfo.InvariantCulture),
                    Volume = volume.ToString(CultureInfo.InvariantCulture) + " BTC",
                    Change = change.ToString("N2")
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
    }
}