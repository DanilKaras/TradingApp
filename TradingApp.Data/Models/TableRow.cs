namespace TradingApp.Data.Models
{
    public class TableRow
    {
        public string Id { get; set; }
        public string Ds { get; set; }
        public decimal Yhat { get; set; }
        public decimal YhatLower { get; set; }
        public decimal YhatUpper { get; set; }
        public decimal MaxVal { get; set; }
        public decimal MinVal { get; set; }
    }
}