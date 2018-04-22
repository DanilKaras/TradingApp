using System.Collections.Generic;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Models;

namespace TradingApp.Core.Core
{
    public static class Shared
    {
        private static readonly object _locker;
        private static List<ExcelLog> _log;

        public static List<ExcelLog> GetLog => _log;
        
        static Shared()
        {
           _locker = new object(); 
           _log = new List<ExcelLog>();
        }
        
        public static void Log(string assetName, Indicator result, decimal rate)
        {
            lock (_locker)
            {
                _log.Add(new ExcelLog()
                {
                    AssetName = assetName,
                    Log = result.ToString(),
                    Rate = rate.ToString()
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