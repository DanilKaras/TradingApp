using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Options;
using TradingApp.Data.Enums;
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
        
//        public bool LoadToCsv(List<CoinOptimized> model)
//        {   
//            var csv = new StringBuilder();
//            const string fs = "Time";
//            const string sc = "avg";    
//            var beginning = $"{fs},{sc}{Environment.NewLine}";
//            
//            //var location = manager.GenerateForecastFolder(coinName, period, DirSwitcher.Manual);
//            
//            csv.Append(beginning);
//            var counter = 0;
//            var maxcount = 0;
//           
//            foreach (var item in model)
//            {                
//                maxcount++;
//            }
//            foreach (var subModel in model)
//            {
//               
//                    counter++;
//             
//                    //var d2 = StaticUtility.TimeConverter(subModel.TimeClose).ToLocalTime();
//
//                    var formattedDate = subModel.Time.ToString("u").Replace("Z", "");// + " UTC";
//                    decimal avg = 0;
//                    try
//                    {
//                        checked
//                        {
//                            avg = ((Convert.ToInt32(subModel.Close) +
//                                           Convert.ToInt32(subModel.High) +
//                                           Convert.ToInt32(subModel.Low)) / 3) * 100;
//                        }
//                    }
//                    catch (OverflowException e)
//                    {
//                        Console.WriteLine("CHECKED and CAUGHT:  " + e.ToString());
//                    }
//                    var second = avg.ToString(CultureInfo.CurrentCulture);
//                    var newLine = counter < maxcount ? $"{formattedDate},{second}{Environment.NewLine}" : $"{formattedDate},{second}";
//                    csv.Append(newLine);
//                
//            }
//
//            //var saveTo = Path.Combine(location, _dataFileName);
////            if (csv.Length == 0)
////            {
////                lock (_locker)
////                {
////                    DirectoryManager.RemoveFolder(saveTo);
////                    return false;
////                }
////            }
//
//            lock (_locker)
//            {
//                File.WriteAllText(saveTo, csv.ToString());
//                return true;
//            }
//        }
        
    }
}