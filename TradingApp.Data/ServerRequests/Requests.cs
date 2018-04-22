using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TradingApp.Data.Utility;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models.CoinOptimizationRelated;
using TradingApp.Domain.Models.ServerRelated;

namespace TradingApp.Data.ServerRequests
{
    public class Requests : IRequests
    {
        private readonly WebClient _client;
        private readonly Random _random;

        public Requests()
        {
            _client = new WebClient();
            _random = new Random();
        }

        public ExchangeData GetAssets(string exhangeName)
        {
            var responseString = _client.DownloadString(Static.ExchanesLink);
            var converter = new ExpandoObjectConverter();
            var responseObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
            var exchange = new ExchangeData
            {
                ExchangeName = exhangeName,
                Pairs = new List<ExchangeCurrency>(),
                Btc = string.Empty
            };
            try
            {
                foreach (var subExchange in responseObj)
                {
                    if (subExchange.Key.Equals(exhangeName))
                    {
                        foreach (var pair in (ExpandoObject) subExchange.Value)
                        {
                            if (pair.Key.Equals(Static.Btc))
                            {
                                var currFrom = pair.Key;
                                foreach (var currency in (IList) pair.Value)
                                {
                                    var currTo = (string) currency;
                                    if (currTo.Contains(Static.Usd))
                                    {
                                        exchange.Btc = currFrom + "_" + currTo + "_" + exhangeName;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var currency in (IList) pair.Value)
                                {
                                    if (currency.Equals(Static.Btc))
                                    {
                                        exchange.Pairs.Add(new ExchangeCurrency()
                                        {
                                            CurrencyFrom = pair.Key,
                                            CurrencyTo = currency.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return exchange;
        }

        public List<string> GetExchanges()
        {
            var responseString = _client.DownloadString(Static.ExchanesLink);
            var converter = new ExpandoObjectConverter();
            var responseObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString, converter);
            var allExchanges = new List<string>();

            try
            {
                allExchanges.AddRange(responseObj.Select(subExchange => subExchange.Key));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return allExchanges;
        }

        public ServerRequestsStats GetStats()
        {
            var responseString = _client.DownloadString(Static.StatsLink);
            var stats = JsonConvert.DeserializeObject<ServerRequestsStats>(responseString);
            return stats;
        }

        public CoinModel GetCoinData(string symbol)
        {
            var client = new WebClient();
            var symbolParts = ParseSymbol(symbol);

            var requestString = Static.GetCoinDataLink +
                                "fsym=" + symbolParts.FromSymbol +
                                "&tsym=" + symbolParts.ToSymbol +
                                "&limit=2000&" +
                                "&e=" + symbolParts.Exhange;
            
            Thread.Sleep(_random.Next(200, 3500));
            var responseString = client.DownloadString(requestString);

            var responseObj = JsonConvert.DeserializeObject<CoinModel>(responseString);
            
            return responseObj;
        }

        private static SymbolForRequest ParseSymbol(string symbol)
        {
            var parts = symbol.Split('_');
            
            var model = new SymbolForRequest()
            {
                FromSymbol = parts[0],
                ToSymbol = parts[1],
                Exhange =  parts[2]
            };

            return model;
        }
    }
}