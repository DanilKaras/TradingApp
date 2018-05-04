using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

        public decimal Rsi(List<CoinOptimized> coin)
        {
            var change = new List<decimal>();
            var gain = new List<decimal>();
            var loss = new List<decimal>();
            for (var i = 1; i < coin.Count; i++)
            {
                change.Add(decimal.Parse(coin[i].Close, NumberStyles.Any, CultureInfo.InvariantCulture) - 
                            decimal.Parse(coin[i-1].Close, NumberStyles.Any, CultureInfo.InvariantCulture));
                //change.Add(Shared.CLOSE[i] - Shared.CLOSE[i - 1]);
            }

            foreach (var item in change)
            {
                if (item < 0)
                {
                    gain.Add(0);
                    loss.Add(-item);
                }
                else if(item > 0)
                {
                    gain.Add(item);
                    loss.Add(0);
                }
                else
                {
                    gain.Add(0);
                    loss.Add(0);
                }
            }

            var avgGain = Average(gain);
            var avgLoss = Average(loss);
            
            return CalculateRsi(avgGain, avgLoss);
        }

        private static List<decimal> Average(IReadOnlyList<decimal> list)
        {
            var avg = new List<decimal>();
            const int start = 14;
            var firstElement = list.Take(start).Average();
            avg.Add(firstElement);
            for (var i = start; i < list.Count; i++)
            {
                var next = (avg[i - start] * 13 + list[i]) / 14;
                avg.Add(next);
            }
            
            
            return avg;
        }
        
        
        private static decimal CalculateRsi(IReadOnlyList<decimal> avgGain, IReadOnlyList<decimal> avgLoss)
        {
            var rsi = new List<decimal>();
            for (var i = 0; i < avgLoss.Count; i++)
            {   
                if (avgLoss[i] == 0)
                {
                    rsi.Add(100);
                }
                else
                {
                    var rs = avgGain[i]/avgLoss[i];
                    rsi.Add(100 - 100 / (1 + rs));
                }
            }

            return rsi.Last();
        }
    }
}