using Newtonsoft.Json;

namespace TradingApp.Domain.Telegram
{
    public class TelegramChat
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}