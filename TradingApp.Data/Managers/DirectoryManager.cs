using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using TradingApp.Data.Utility;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.CoinOptimizationRelated;

namespace TradingApp.Data.Managers
{
    public class DirectoryManager : IDirectoryManager
    {
        private readonly string _todayDate;
        private readonly string _location;
        private readonly string _env;
        private static object _locker;
        private readonly string _manual;
        private readonly string _automatic;
        private readonly string _fixedAssets;
        private readonly string _observable;
        private readonly string _subFolderForAuto;
        private readonly string _instant;
        private readonly int _timeName;
        private readonly string _customSettings;
        private readonly string _dataFileName;
        private readonly string _dirNegative;
        private readonly string _dirNeutral;
        private readonly string _dirPositive;
        private readonly string _dirStrongPositive;

        public string Location => _location;
        public string AsstesUpdateLocation => GetAsstesUpdateLocation();
        public string AsstesLocation => GetAsstesLocation();
        public string ObservablesLocationUpdate => GetObservableLocationUpdate();
        public string ObservablesLocation => GetObservableLocation();
        public string CustomSettings => CustomSettingsContent();
        public string DirNegative => _dirNegative;

        public string DirPositive => _dirPositive;

        public string DirNeutral => _dirNeutral;

        public string DirStrongPositive => _dirStrongPositive;

        private readonly ISettings _settings;
        private readonly IFileManager _fileManager;
        
        public DirectoryManager(ISettings settings, IFileManager fileManager)
        {
            _fileManager = fileManager;
            _settings = settings; 
           
            _locker = new object();
            _todayDate = DateTime.Today.Date.ToString("dd-MM-yy");          
            _env = settings.CurrentLocation;
            _location = Dir();
            _manual = _settings.ManualFolder;
            _automatic = _settings.AutoFolder;
            _instant = _settings.InstantFolder;
            _fixedAssets = _settings.AssetFile;
            _observable = settings.ObservableFile;
            _subFolderForAuto = DateTime.Now.ToString("HH:mm:ss").Replace(':', '-');
            _timeName = 9;
            _customSettings = _settings.CustomSettings;
            _dataFileName = "data.csv";
            _dirNegative = Indicator.Negative.ToString();
            _dirNeutral = Indicator.Neutral.ToString();
            _dirPositive = Indicator.Positive.ToString();
            _dirStrongPositive = Indicator.StrongPositive.ToString(); 
        }
        
        
        private string Dir()
        {
            var rootLocation = Path.Combine(Directory.GetCurrentDirectory(), _settings.ForecastDir);
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
                throw new Exception("Couldn't find " + _fixedAssets);
            }
            return path;
        }

        private string GetObservableLocationUpdate()
        {
            var path = Path.Combine(_env, _observable);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }
        
        private string GetObservableLocation()
        {
            var path = Path.Combine(_env, _observable);
            if (!File.Exists(path))
            {
                throw new Exception("Couldn't find "+ _observable);
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
                    var defaultSettings = new CustomSettings();
                    var defaultJson = _fileManager.ConvertCustomSettings(defaultSettings);
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
        
        
        public string GenerateForecastFolder(string assetId, int period, DirSwitcher switcher, DateTime? context = null)
        {
            var timeNow = DateTime.Now.ToString("HH:mm:ss").Replace(':', '.');
            var newFolder = $"{timeNow}_{assetId}_{period}";
            string newLocation;
            
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    var subFolderForAuto = context?.ToString("HH:mm:ss").Replace(':', '-');
                    newLocation = Path.Combine(_location, _automatic, subFolderForAuto, newFolder);
                    break;
                case DirSwitcher.Manual:
                    newLocation = Path.Combine(_location, _manual, newFolder);
                    break;
                case DirSwitcher.Instant:
                    newLocation = Path.Combine(_location, _instant, newFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
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

        public void SaveDataFile(string content, string location)
        {
            var saveTo = Path.Combine(location, _dataFileName);
            lock (_locker)
            {
                File.WriteAllText(saveTo, content);
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
        
        
        public ImagesPath ImagePath (DirSwitcher switcher, Indicator? indicator = null, string subFolder = null, string fullPath = null)
        {
            string tmpCurrent;
            string path;
            var images = new ImagesPath();
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    var parts = _location.Split(Path.DirectorySeparatorChar);
                    Array.Resize(ref parts, parts.Length - 1);
                    var locStr = new StringBuilder();
                    foreach (var part in parts)
                    {
                        locStr.Append(part); //, Path.DirectorySeparatorChar.ToString());
                        locStr.Append(Path.DirectorySeparatorChar.ToString());
                    }

                    var loc = locStr.ToString();
                    var time = fullPath.Split(Path.DirectorySeparatorChar);
                    tmpCurrent = LastDirAuto(Path.Combine(loc)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(tmpCurrent, _automatic);
                    images.ForecastImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        path, 
                        time.Last(),
                        indicator.ToString(),
                        subFolder,
                        Static.ForecastFile);
                    images.ComponentsImage =  Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        path, 
                        time.Last(),
                        indicator.ToString(),
                        subFolder,
                        Static.ComponentsFile);
                    break;
                case DirSwitcher.Manual:
                    tmpCurrent = LastDir(Path.Combine(_location, _manual)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(_manual, tmpCurrent);
                    images.ForecastImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ForecastFile);
                    images.ComponentsImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ComponentsFile);
                    break;
                case DirSwitcher.Instant:
                    tmpCurrent = LastDir(Path.Combine(_location, _instant)).Split(Path.DirectorySeparatorChar).Last();
                    path = Path.Combine(_instant, tmpCurrent);
                    images.ForecastImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ForecastFile);
                    images.ComponentsImage = Path.Combine(Path.DirectorySeparatorChar.ToString(), 
                        _settings.ForecastDir, 
                        _todayDate,
                        path, 
                        Static.ComponentsFile);
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
        
        private static string LastDirAuto(string dir)
        {
            var lastHigh = new DateTime(1900,1,1);
            var highDir = string.Empty;
            foreach (var subdir in Directory.GetDirectories(dir))
            {
                var file = new DirectoryInfo(subdir);
                var created = file.LastWriteTime;

                if (created <= lastHigh) continue;
                highDir = subdir;
                lastHigh = created;
            }
            return highDir;
        }
        
        public bool SpecifyDirByTrend(Indicator switcher, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;
                var getUpperFolder = path.Remove(0,path.LastIndexOf(Path.DirectorySeparatorChar)+1);
                var getRootPath = path.Remove(path.LastIndexOf(Path.DirectorySeparatorChar), path.Length - path.LastIndexOf(Path.DirectorySeparatorChar));
                string moveTo;
                switch(switcher)
                {
                    case Indicator.Positive:
                        moveTo = CreateSubDir(getRootPath, _dirPositive);
                        break;
                    case Indicator.Neutral:
                        moveTo = CreateSubDir(getRootPath, _dirNeutral);
                        break;
                    case Indicator.Negative:
                        moveTo = CreateSubDir(getRootPath, _dirNegative);
                        break;
                    case Indicator.StrongPositive:
                        moveTo = CreateSubDir(getRootPath, _dirStrongPositive);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
                }

                if (!string.IsNullOrEmpty(moveTo))
                {
                    MoveFolderToDir(path, moveTo, getUpperFolder);
                    return true;
                }                
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return false;
        }
        
        private static string CreateSubDir(string path, string folderName)
        {
            var newPath = Path.Combine(path, folderName);
            bool exist;
            lock (_locker)
            {
                exist = Directory.Exists(newPath);
            }

            if (exist) return newPath;
            lock (_locker)
            {
                Directory.CreateDirectory(newPath);
            }
            return newPath;
        }
        
        private static void MoveFolderToDir(string moveFrom, string moveTo, string oldFolderName)
        {
            var folderWithOldName = CreateSubDir(moveTo, oldFolderName);
            string[] files;
            lock (_locker)
            {
                files = Directory.GetFiles(moveFrom);
            }

            foreach (var s in files)
            {
                lock (_locker)
                {
                    var fileName = Path.GetFileName(s);
                    var destFile = Path.Combine(folderWithOldName, fileName);
                    File.Copy(s, destFile, true);
                }
            }

            lock (_locker)
            {
                if (!Directory.Exists(moveFrom)) return;
            }

            try
            {
                lock (_locker)
                {
                    Directory.Delete(moveFrom, true);
                }   
            }
            catch (IOException e)
            {
                throw new Exception(e.Message);
            }
        }
        
        public string GetLastFolder(DirSwitcher switcher)
        {
            var parts = _location.Split(Path.DirectorySeparatorChar);
            Array.Resize(ref parts, parts.Length - 1);
            var locStr = new StringBuilder();
            foreach (var part in parts)
            {
                locStr.Append(part); //, Path.DirectorySeparatorChar.ToString());
                locStr.Append(Path.DirectorySeparatorChar.ToString());
            }

            var loc = locStr.ToString();
            
            switch (switcher)
            {
                case DirSwitcher.Auto:
                    
                    loc = Path.Combine(LastDirAuto(loc), _automatic);
                    break;
                case DirSwitcher.Manual:
                    loc = Path.Combine(LastDir(loc), _manual);
                    break;
                case DirSwitcher.Instant:
                    loc = Path.Combine(LastDir(loc), _instant);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(switcher), switcher, null);
            }
            return LastDir(loc);
        }
        
        public void WriteLogToExcel(string path, IEnumerable<ExcelLog> log)
        {
            var fileDestination = Path.Combine(path, _settings.AssetFile);
            var file = new FileInfo(fileDestination);
            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                var rowNumber = 1;
                foreach (var subLog in log)
                {
                    worksheet.Cells[rowNumber, 1].Value = subLog.AssetName;
                    worksheet.Cells[rowNumber, 2].Value = subLog.Log;   
                    worksheet.Cells[rowNumber, 3].Value = subLog.Rate;
                    worksheet.Cells[rowNumber, 4].Value = subLog.Change;
                    worksheet.Cells[rowNumber, 5].Value = subLog.Volume;
                    worksheet.Cells[rowNumber, 6].Value = subLog.Rsi;
                    rowNumber++;
                }
                package.Save();
            }
        }

        
        
        public AsstesByIndicator GetListByIndicator(string folder)
        {
            var model = new AsstesByIndicator();
        
            var positiveDir = Path.Combine(folder, _dirPositive);
            var neutralDir = Path.Combine(folder, _dirNeutral);
            var negativeDir = Path.Combine(folder, _dirNegative);
            var strongPositiveDir = Path.Combine(folder, _dirStrongPositive);
            
            if (Directory.Exists(positiveDir))
            {
                model.PositiveAssets = GetFolderNames(positiveDir);
            }

            if (Directory.Exists(neutralDir))
            {
                model.NeutralAssets = GetFolderNames(neutralDir);
            }
            
            if (Directory.Exists(negativeDir))
            {
                model.NegativeAssets = GetFolderNames(negativeDir);
            }
            
            if (Directory.Exists(strongPositiveDir))
            {
                model.StrongPositiveAssets = GetFolderNames(strongPositiveDir);
            }

            return model;
        }

        public List<ExcelLog> GetReport(string folder)
        {
            
            var file = Path.Combine(folder, _settings.AssetFile);
            if (File.Exists(file))
            {
                return _fileManager.ReadLog(file);
            }

            return null;
        }
        
        public string GetDirByIndicator(string folder, Indicator indicator)
        {
            var indicatorDir = indicator.ToString();
            string dir;
            switch (indicator)
            {
                case Indicator.Positive:
                    dir = Path.Combine(folder, indicatorDir);
                    break;
                case Indicator.Neutral:
                    dir = Path.Combine(folder, indicatorDir);
                    break;
                case Indicator.Negative:
                    dir = Path.Combine(folder, indicatorDir);
                    break;
                case Indicator.StrongPositive:
                    dir = Path.Combine(folder, indicatorDir);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(indicator), indicator, null);
            }

            return dir;
        }
        
        private List<string> GetFolderNames(string dir)
        {
            var names = new List<string>();
            if (!Directory.Exists(dir)) return names;
            var files = Directory.GetDirectories(dir);
            foreach (var folder in files)
            {
                var lastFolder = folder.Split(Path.DirectorySeparatorChar).Last();
                var name = lastFolder.Substring(_timeName, lastFolder.Length - (_timeName + 3));
                names.Add(name);
            }
            return names;
        }
        
        public string GetForecastFolderByName(string dir, string assetName)
        {
            try
            {
                string name;
                var files = Directory.GetDirectories(dir);
                return files.FirstOrDefault(x => x.Contains(assetName))?.Split(Path.DirectorySeparatorChar).Last();
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't Get Forecast Folder in {dir} by Name {assetName}");
            }           
        }
        
        public void RemoveFolder(string path)
        {
            if (!Directory.Exists(path)) return;
            lock (_locker)
            {
                Directory.Delete(path, true); 
            }
        }
    }
}