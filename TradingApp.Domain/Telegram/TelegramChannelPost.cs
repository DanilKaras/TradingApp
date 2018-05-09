using System;
using Newtonsoft.Json;

namespace TradingApp.Domain.Telegram
{
    public class TelegramChannelPost
    {
        [JsonProperty("message_id")]
        public long MessageId { get; set; }
        
        [JsonProperty("date")]
        public string Date { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("chat")]
        public TelegramChat Chat { get; set; }
    }
}