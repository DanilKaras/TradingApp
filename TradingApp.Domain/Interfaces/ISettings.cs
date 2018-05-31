namespace TradingApp.Domain.Interfaces
{
    public interface ISettings
    {
        string ForecastDir { get; set; }
        string BotDir { get; set; }
        string PythonLocation { get; set; }
        string FileName { get; set; }
        string AssetFile { get; set; }
        string ManualFolder { get; set; }
        string AutoFolder { get; set; }
        string InstantFolder { get; set; }
        string BotForecastFolder { get; set; }
        string CustomSettings { get; set; }
        string ObservableFile { get; set; }
        string BotAssetsFile { get; set; }
        string CurrentLocation { get; set; }
        string TelegramApi { get; set; }
        string TelegramChatId { get; set; }
    }
}