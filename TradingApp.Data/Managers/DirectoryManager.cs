using System;
using System.IO;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using TradingApp.Data.Models;

namespace TradingApp.Data.Managers
{
    public class DirectoryManager
    {
        private readonly string _todayDate;
        private readonly IOptions<ApplicationSettings> _settings;
        private readonly string _location;
        private readonly string _env;
        private static object _locker;
        private readonly string _manual;
        private readonly string _automatic;
        private readonly string _fixedAssets;
        private readonly string _subFolderForAuto;
        private readonly string _instant;
        private static int _timeName;
        private readonly string _customSettings;
        
        public string Location => _location;
        public string AsstesLocation => GetAsstesLocation();
        public string CustomSettings => CustomSettingsContent();
        
        public DirectoryManager(IOptions<ApplicationSettings> settings, string env)
        {
            _locker = new object();
            _todayDate = DateTime.Today.Date.ToString("dd-MM-yy");          
            _settings = settings;
            _env = env;
            _location = Dir();
            _manual = _settings.Value.ManualFolder;
            _automatic = _settings.Value.AutoFolder;
            _instant = _settings.Value.InstantFolder;
            _fixedAssets = _settings.Value.AssetFile;
            _subFolderForAuto = DateTime.Now.ToString("HH:mm:ss").Replace(':', '-');
            _timeName = 12;
            _customSettings = _settings.Value.CustomSettings;
        }
        
        
        private string Dir()
        {
            var rootLocation = Path.Combine(Directory.GetCurrentDirectory(), _settings.Value.ForecastDir);
            var newLocation = Path.Combine(rootLocation, _todayDate);
            var exist = Directory.Exists(newLocation);
            if (exist) return newLocation;
            lock (_locker)
            {
                Directory.CreateDirectory(newLocation);
            }
            return newLocation;
        }

        private string GetAsstesLocation()
        {
            var path = Path.Combine(_env, _fixedAssets);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }

        private string CustomSettingsContent()
        {
            var path = Path.Combine(_env, _customSettings);
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            
            string json;
            using (var content = new StreamReader(path))
            {
                 json = content.ReadToEnd();
            }
            return json;
        }
        
        
        public void UpdateCustomSettings(string json)
        {
            var path = Path.Combine(_env, _customSettings);
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            File.WriteAllText(path, json);
        }
    }
}