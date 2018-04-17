using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradingApp.Data.Enums;
using TradingApp.Data.Models;
using TradingApp.Data.Utility;

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
        private readonly string _dataFileName;
        public string Location => _location;
        public string AsstesUpdateLocation => GetAsstesUpdateLocation();
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
            _dataFileName = "data.csv";
            
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

        private string GetAsstesUpdateLocation()
        {
            var path = Path.Combine(_env, _fixedAssets);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }
        
        private string GetAsstesLocation()
        {
            var path = Path.Combine(_env, _fixedAssets);
            if (!File.Exists(path))
            {
                throw new Exception("Cannot find the assets.xlxs");
            }
            return path;
        }

        private string CustomSettingsContent()
        {
            var path = Path.Combine(_env, _customSettings);
            try
            {
                if (!File.Exists(path))
                {
                    var myFile = File.Create(path);
                    myFile.Close();

                    var manager = new FileManager(_settings);
                    var defaultSettings = new CustomSettings();
                    var defaultJson = manager.ConvertCustomSettings(defaultSettings);
                    File.WriteAllText(path, defaultJson);
                }
                string json;
                using (var content = new StreamReader(path))
                {
                    json = content.ReadToEnd();
                }
                return json;
            }
            catch (Exception e)
            {
                
                throw new Exception(e.Message);
            }           
        }
        
        
        public void UpdateCustomSettings(string json)
        {
            var path = Path.Combine(_env, _customSettings);
            if (!File.Exists(path))
            {
                var file = File.Create(path);
                file.Close();
            }
            
            File.WriteAllText(path, json);
        }
        
        
        public string GenerateForecastFolder(string assetId, int period, DirSwitcher switcher)
        {
            var timeNow = DateTime.Now.ToString("HH:mm:ss").Replace(':', '.');
            var newFolder = $"{timeNow}_{assetId}_{period}";
            string newLocation;
            ///TODO
            newLocation = Path.Combine(_location, _manual, newFolder);
//            switch (switcher)
//            {
//                case DirSwitcher.Auto:
//                    newLocation = Path.Combine(this.location, automatic, subFolderForAuto, newFolder);
//                    break;
//                case DirSwitcher.Manual:
//                   newLocation = Path.Combine(_location, _manual, newFolder);
//                    break;
//                case DirSwitcher.Instant:
//                    newLocation = Path.Combine(this.location, instant, newFolder);
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
//            }

            try
            {
                var exist = Directory.Exists(newLocation);
                if (exist) return newLocation;
                lock (_locker)
                {
                    Directory.CreateDirectory(newLocation);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't generate the Forecast Folder");
            }
           
            return newLocation;
        }

        public void SaveDataFile(StringBuilder content, string location)
        {
            var saveTo = Path.Combine(location, _dataFileName);
            lock (_locker)
            {
                File.WriteAllText(saveTo, content.ToString());
            }
        }
        
        public async Task<bool> WaitForFile(string path, int timeout)
        {
            var timeoutAt = DateTime.Now.AddSeconds(timeout);
            while (true)
            {
                if (File.Exists(path)) return true;
                if (DateTime.Now >= timeoutAt) return false;
                await Task.Delay(10);
            }
        }


        public string FilePathOut(string currentForecastDir)
        {
            return Path.Combine(currentForecastDir, Static.OutFile);
        }
        
        public string FileForecastOut(string currentForecastDir)
        {
            return Path.Combine(currentForecastDir, Static.ForecastFile);
        }
        
        public string FileComponentsOut(string currentComponentsDir)
        {
            return Path.Combine(currentComponentsDir, Static.ComponentsFile);
        }
        
        
        public ImagesPath ImagePath (DirSwitcher switcher)
        {
            string tmpCurrent;
            string path;
            var images = new ImagesPath();
            //TODO make location crossplatform
            //var tmpTodayFolder = _location;//location.Replace("//", "/").Split('/').Last();
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    tmpCurrent = LastDir(Path.Combine(_location, _automatic)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(_automatic, tmpCurrent);
                    break;
                case DirSwitcher.Manual:
                    tmpCurrent = LastDir(Path.Combine(_location, _manual)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(_manual, tmpCurrent);
                    images.ForecastImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.Value.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ForecastFile);
                    images.ComponentsImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.Value.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ComponentsFile);
                    break;
                case DirSwitcher.Instant:
                    tmpCurrent = LastDir(Path.Combine(_location, _instant)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(_instant, tmpCurrent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
            
            
            return images;
        }
        
        private static string LastDir(string dir)
        {
            var lastHigh = new DateTime(1900,1,1);
            var highDir = string.Empty;
            foreach (var subdir in Directory.GetDirectories(dir))
            {
                var file = new DirectoryInfo(subdir);
                var created = file.CreationTime;

                if (created <= lastHigh) continue;
                highDir = subdir;
                lastHigh = created;
            }
            
            return highDir;
        }
    }
}