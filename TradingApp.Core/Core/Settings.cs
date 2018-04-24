using Microsoft.Extensions.Configuration;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;

namespace TradingApp.Core.Core
{
    public class Settings : ISettings
    {
        private IConfiguration Configuration { get; set; }

        public Settings(IConfiguration configuration)
        {
            Configuration = configuration;
            ForecastDir = Configuration["ApplicationSettings:ForecastDir"];
            PythonLocation = Configuration["ApplicationSettings:PythonLocation"];
            FileName = Configuration["ApplicationSettings:FileName"];
            AssetFile = Configuration["ApplicationSettings:AssetFile"];
            ManualFolder = Configuration["ApplicationSettings:ManualFolder"];
            AutoFolder = Configuration["ApplicationSettings:AutoFolder"];
            InstantFolder = Configuration["ApplicationSettings:InstantFolder"];
            CustomSettings = Configuration["ApplicationSettings:CustomSettings"];
            ObservableFile = Configuration["ApplicationSettings:ObservableFile"];
        }

        public string ForecastDir { get; set; }
        public string PythonLocation { get; set; }
        public string FileName { get; set; }
        public string AssetFile { get; set; }
        public string ManualFolder { get; set; }
        public string AutoFolder { get; set; }
        public string InstantFolder { get; set; }
        public string CustomSettings { get; set; }
        public string ObservableFile { get; set; }
    }
}