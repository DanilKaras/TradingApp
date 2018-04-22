﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private IOptions<ApplicationSettings> _appSettings;

        public ProcessModel(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings;
        }
        
        
        public List<CoinOptimized> GetDataManual(string symbol, int dataHours)
        {
            IRequests request = new Requests();
            var model = request.GetCoinData(symbol);
            
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
            IRequests request = new Requests();
            var model = request.GetCoinData(symbol);
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
            //return normalized ?? null;
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
            //delete extra elements from data array
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

            return true;
        }
    }
}