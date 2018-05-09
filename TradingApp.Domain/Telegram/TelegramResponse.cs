using System.Collections.Generic;
using Newtonsoft.Json;

namespace TradingApp.Domain.Telegram
{
    public class TelegramResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }
        
        [JsonProperty("result")]
        public List<TelegramResult> Result { get; set; }
    }
}