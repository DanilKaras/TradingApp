using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Options;
using TradingApp.Data.ServerRequests;
using TradingApp.Data.Utility;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;
using TradingApp.Domain.Models.ServerRelated;

namespace TradingApp.Core.Core
{
    public class ProcessModel : IProcessModel
    {
        private readonly IRequests _requestHelper;
        
        public ProcessModel(IRequests requests)
        {
            _requestHelper = requests;
        }
        public List<CoinOptimized> GetDataManual(string symbol, int dataHours)
        {
            
            var model = _requestHelper.GetCoinData(symbol);
            
            if (!model.Response.Equals(Static.StatusSuccess))
            {
                throw new Exception("Bad Data for: " + symbol);
            }
            
            var normalized = NormilizeModel(model, dataHours);
            
            if (normalized == null)
            {
                throw new Exception("Bad Data for: " + symbol); 
            }
            
            return normalized;
        }
        
        public List<CoinOptimized> GetDataAuto(string symbol, int dataHours)
        {
           
            var model = _requestHelper.GetCoinData(symbol);
            if (model?.Response == null)
            {
                return null;
            }
            if (!model.Response.Equals(Static.StatusSuccess))
            {
                return null;
            }
            
            var normalized = NormilizeModel(model, dataHours);
            if (normalized!= null && normalized.Any())
            {
                return normalized;
            }
            return null;
        }

        private List<CoinOptimized> NormilizeModel(CoinModel coin, int dataHours)
        {
            if (coin?.Data == null || 
                coin.Data.Count == 0 || 
                coin.Data.Count < dataHours)
            {
                return null;
            }           
             
            var converted = CovertCoinDateTime(coin, dataHours);
            return converted;
        }

        private static List<CoinOptimized> CovertCoinDateTime(CoinModel coin, int dataHours)
        {
            var optimized = new List<CoinOptimized>();

            coin = RemoveExcess(coin, dataHours);
            
            foreach (var item in coin.Data)
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                optimized.Add(new CoinOptimized()
                {
                    Time = dtDateTime.AddSeconds(Convert.ToDouble(item.Time)).ToLocalTime(),
                    Close = item.Close,
                    High = item.High,
                    Low = item.Low,
                    Open = item.Open,
                    VolumeTo = item.VolumeTo,
                    VolumeFrom = item.VolumeFrom
                });
            }

            var isValid = CheckForGaps(optimized);
            if (isValid)
            {
                return optimized;
            }
            return null;
        }
        
        private static CoinModel RemoveExcess(CoinModel coin, int dataHours)
        {
            coin.Data = coin.Data.Skip(Math.Max(0, coin.Data.Count() - dataHours)).ToList();
            return coin;
        }


        private static bool CheckForGaps(IReadOnlyList<CoinOptimized> optimizedCoin)
        {
            for (var i = 0; i < optimizedCoin.Count - 1; i++)
            {
                var first = optimizedCoin[i];
                var next = optimizedCoin[i + 1];
                if (first.Time.AddDays(1) < next.Time)
                {
                    return false;
                }
            }

            var penaltyDays = 0;
            penaltyDays = optimizedCoin.Count < 576 ? 24 : 48;
            if (optimizedCoin[0].VolumeFrom == "0" ||
                decimal.Parse(optimizedCoin[0].Close, NumberStyles.Any, CultureInfo.InvariantCulture) == 
                 decimal.Parse(optimizedCoin[1].Close, NumberStyles.Any, CultureInfo.InvariantCulture))
            {
                var missingCounter = 1;
                for (var i = 1; i < optimizedCoin.Count - 1; i++)
                {
                    if (optimizedCoin[i].VolumeFrom == "0")
                    {
                        missingCounter++;
                    }

                    if (missingCounter >= penaltyDays)
                    {
                        return false;
                    }
                }
            }
           
            return true;
        }
    }
}