using Newtonsoft.Json;

namespace TradingApp.Domain.Models.ServerRelated
{
    public class CoinData
    {
        [JsonProperty("time")]
        public string Time { get; set; }
        [JsonProperty("close")]
        public string Close { get; set; }
        [JsonProperty("high")]
        public string High { get; set; }
        [JsonProperty("low")]
        public string Low { get; set; }
        [JsonProperty("open")]
        public string Open { get; set; }
        [JsonProperty("volumefrom")]
        public string VolumeFrom { get; set; }
        [JsonProperty("volumeto")]
        public string VolumeTo { get; set; }
    }
}