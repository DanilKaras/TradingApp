using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using TradingApp.Data.Utility;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models.CoinOptimizationRelated;
using TradingApp.Domain.Models.ServerRelated;
using Microsoft.Extensions.Logging;

namespace TradingApp.Data.ServerRequests
{
    public class Requests : IRequests
    {
        private readonly Random _random;
        private readonly ILogger _logger;
        public Requests(ILoggerFactory logger)
        {
            
            _random = new Random();
            _logger = logger.CreateLogger("Requests");
        }

        public ExchangeData GetAssets(string exhangeName)
        {
            var client = new RestClient(Static.ExchanesLink);
            var request = new RestRequest(Method.GET);
            var responseString = client.Execute(request);
            var converter = new ExpandoObjectConverter();
            var responseObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString.Content, converter);
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
                _logger.LogError($"Error while getting assets for {exhangeName}");
                throw new Exception(e.Message);
                
            }

            return exchange;
        }

        public List<string> GetExchanges()
        {
            var client = new RestClient(Static.ExchanesLink);
            var request = new RestRequest(Method.GET);
            var responseString = client.Execute(request);
            var converter = new ExpandoObjectConverter();
            var responseObj = JsonConvert.DeserializeObject<ExpandoObject>(responseString.Content, converter);
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
            try
            {
                var client = new RestClient(Static.StatsLink);
                var request = new RestRequest(Method.GET);
                var responseString = client.Execute(request);
                var stats = JsonConvert.DeserializeObject<ServerRequestsStats>(responseString.Content);
                return stats;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error while getting request stats");
                throw;
            }
            
        }

        public CoinModel GetCoinData(string symbol)
        {
            
            var symbolParts = ParseSymbol(symbol);

            var requestString = Static.GetCoinDataLink +
                                "fsym=" + symbolParts.FromSymbol +
                                "&tsym=" + symbolParts.ToSymbol +
                                "&limit=2000&" +
                                "&e=" + symbolParts.Exhange;
            var request = (HttpWebRequest)WebRequest.Create(requestString);
            request.Timeout = 300000;
            var sleepTime = _random.Next(200, 3500);
            Thread.Sleep(sleepTime);
            try
            {
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream(),true))
                    {
                        var responseString = streamReader.ReadToEnd();
                        var responseObj = JsonConvert.DeserializeObject<CoinModel>(responseString);
                        return responseObj;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error getting coindata for {symbol}, " +
                                 $"random sleep time {sleepTime}, " +
                                 $"thread id {Thread.CurrentThread.ManagedThreadId}, " +
                                 $"generated request link {request}");
                return null;
            }
            
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