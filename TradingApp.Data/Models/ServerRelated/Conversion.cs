using Newtonsoft.Json;

namespace TradingApp.Data.Models
{
    public class Conversion
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("conversionSymbol")]
        public string conversionSymbol { get; set; }
    }
}