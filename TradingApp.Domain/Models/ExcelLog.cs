namespace TradingApp.Domain.Models
{
    public class ExcelLog
    {
        public string AssetName { get; set; }
        public string Log { get; set; }
        public string Rate { get; set; }
        public string Volume { get; set; }
        public string Change { get; set; }
    }
}