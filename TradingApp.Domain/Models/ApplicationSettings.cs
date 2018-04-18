namespace TradingApp.Domain.Models
{
    public class ApplicationSettings
    {
        public string ForecastDir { get; set; }
        public string PythonLocation { get; set; }
        public string FileName { get; set; }
        public string AssetFile { get; set; }
        public string ManualFolder { get; set; }
        public string AutoFolder { get; set; }
        public string InstantFolder { get; set; }
        public string CustomSettings { get; set; }
    }
}