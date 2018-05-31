namespace TradingApp.Core.BotTools
{
    public interface IFireScheduler
    {
        void FireForecaster();
        void StopForecaster();
        void TriggerImmediately();
        void FireForecasterSecond();
    }
}