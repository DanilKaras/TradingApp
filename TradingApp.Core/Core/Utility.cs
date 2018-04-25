using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Core.Core
{
    public class Utility : IUtility
    {
        private readonly CustomSettings _settings;
        
        public Utility(IFileManager fileManager, IDirectoryManager directoryManager)
        {
            _settings = fileManager.ReadCustomSettings(directoryManager.CustomSettings);
        }
        
        public CoinPerformance DefinePerformance(OutStats table)
        {
            var tableRows = table.Table;
            var result = new CoinPerformance();
            
            if (!decimal.TryParse(_settings.LowerBorder, out var lowerBorder))
            {
                throw new Exception("Wrong Value of Border in App Settings!");
            }
            
            if (!decimal.TryParse(_settings.UpperBorder, out var upperBorder))
            {
                throw new Exception("Wrong Value of BorderUp in App Settings!");
            }

            var enumerable = tableRows.ToList();
            var upper = enumerable.First().Yhat;
            var lower = enumerable.Last().Yhat;
            var max = table.MaxValue;
            var min = table.MinValue;

            if (max < upper)
            {
                max = upper;
            }

            if (min > lower)
            {
                min = lower;
            }
            
            decimal length;
            if (upper - lower > 0)
            {
                length =  1 / (max - min) * (upper - lower) * 100;
            }
            else if(upper - lower < 0)
            {
                length = -1 * (1 / (max - min) * (lower - upper) * 100);
            }
            else
            {
                result.Indicator = Indicator.Neutral;
                result.Rate = 0;
                return result;
            }

            
            if (length > upperBorder)
            {
                result.Indicator = Indicator.StrongPositive;
                result.Rate = length/100;
                return result;
            }
            
            if (length > 0 && length <= upperBorder)
            {
                result.Indicator = Indicator.Positive;
                result.Rate = length/100;
                return result;
            }
            
            if (length < 0 && lowerBorder < length)
            {
                result.Indicator = Indicator.Neutral;
                result.Rate = -1 * length/100;
                return result;
            }
            
            result.Indicator = Indicator.Negative;
            result.Rate =  - 1 * length / 100;
            return result;
        }

        public MarketFeature GetFeatures(List<CoinOptimized> coin, string coinName)
        {          
            var features = new MarketFeature();
            var changeEnd = default(decimal);
            var changeStart = default(decimal);

            var volumeBtc = default(decimal);

            for (var i = coin.Count - 1; i > (coin.Count - 24) - 1; i--)
            {
                volumeBtc += decimal.Parse(coin[i].VolumeTo, NumberStyles.Any, CultureInfo.InvariantCulture);
                
                if (i == coin.Count - 1)
                {
                    changeEnd = ((decimal.Parse(coin[i].Close, NumberStyles.Any, CultureInfo.InvariantCulture) + 
                                 decimal.Parse(coin[i].High, NumberStyles.Any, CultureInfo.InvariantCulture) + 
                                 decimal.Parse(coin[i].Low, NumberStyles.Any, CultureInfo.InvariantCulture)) / 3) * 100;
                }

                if (i == coin.Count - 24)
                {
                    changeStart = ((decimal.Parse(coin[i].Close, NumberStyles.Any, CultureInfo.InvariantCulture) + 
                                   decimal.Parse(coin[i].High, NumberStyles.Any, CultureInfo.InvariantCulture) + 
                                   decimal.Parse(coin[i].Low, NumberStyles.Any, CultureInfo.InvariantCulture)) / 3) * 100;
                }
            }

            features.Volume = volumeBtc;
            features.Change = (1 - changeStart / changeEnd) * 100;
            features.CoinName = coinName;
            return features;
        }
    }
}