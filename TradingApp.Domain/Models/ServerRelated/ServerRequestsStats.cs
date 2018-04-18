namespace TradingApp.Domain.Models.ServerRelated
{
    public class ServerRequestsStats
    {
        public CallsMade CallsMade { get; set; }
        public CallsLeft CallsLeft { get; set; }
        public string Message { get; set; }
    }
}