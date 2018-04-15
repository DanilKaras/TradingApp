﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TradingApp.Data.Models;
using TradingApp.Data.Utility;

namespace TradingApp.Data.ServerRequests
{
    public class Requests
    {
        private readonly WebClient _client;
        
        public Requests()
        {
            _client = new WebClient();
            
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
        
    }
}