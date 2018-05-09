using Newtonsoft.Json;

namespace TradingApp.Domain.Telegram
{
    public class TelegramResult
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }
        
        [JsonProperty("channel_post")]
        public TelegramChannelPost ChannelPost { get; set; }
    }
}