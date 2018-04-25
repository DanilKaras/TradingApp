using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TradingApp.Data.Managers;
using TradingApp.Data.ServerRequests;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.Models.ServerRelated;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Core.Core
{
    public class Forecaster : IForecaster
    {
        private readonly IProcessModel _processModel;
        private readonly IDirectoryManager _directoryManager;
        private readonly IFileManager _fileManager;
        private readonly IPythonExec _pythonExec;
        private readonly IUtility _utility;
        private readonly IRequests _requestHelper;
        public Forecaster(IProcessModel processModel, IDirectoryManager directoryManager, IFileManager fileManager, IPythonExec pythonExec, IUtility utility, IRequests requests)
        {
            _processModel = processModel;
            _fileManager = fileManager;
            _directoryManager = directoryManager;
            _pythonExec = pythonExec;
            _utility = utility;
            _requestHelper = requests;
        }

        public ServerRequestsStats GetStats()
        {
            var stats = new Requests();
            return stats.GetStats();           
        }

        public async Task<ManualViewModel> MakeManualForecast(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new ManualViewModel();
            
            var directory = _directoryManager;
            var file = _fileManager;
            
            try
            {
                var normalized = _processModel.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Manual);
                
                var csv = _fileManager.CreateDataCsv(normalized, location);
                if (string.IsNullOrEmpty(csv))
                {
                    throw new Exception("Not enough data: " + asset); 
                }
                _directoryManager.SaveDataFile(csv, location);        
                
                _pythonExec.RunPython(location, periods, hourlySeasonality, dailySeasonality);
                
                var pathToOut = directory.FilePathOut(location);
                var pathToComponents = directory.FileComponentsOut(location);
                var pathToForecast = directory.FileForecastOut(location);
                var outCreated = await directory.WaitForFile(pathToOut, 60);
                var componentsCreated = await directory.WaitForFile(pathToComponents, 10);
                var forecastCreated = await directory.WaitForFile(pathToForecast, 10);
                var images = directory.ImagePath(DirSwitcher.Manual);
                
                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    throw new Exception("forecast.png not found");
                }

                if (componentsCreated)
                {
                    viewModel.ComponentsPath = images.ComponentsImage;
                }
                else
                {
                    throw new Exception("components.png not found");
                }

                if (outCreated)
                {
                    var stats = file.BuildOutTableRows(pathToOut, periods);                  
                    var performance = _utility.DefinePerformance(stats);
                    viewModel.Table = stats.Table;
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("N2");
                    var marketFeatures = _utility.GetFeatures(normalized, asset);                   
                    viewModel.Volume = marketFeatures.Volume.ToString();
                    viewModel.Change = marketFeatures.Change.ToString("N2");
                }
                else
                {
                    throw new Exception("out.csv not found");
                }

                viewModel.AssetName = asset;

               
                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                return viewModel;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
          
        public async Task<AutoViewModel> MakeAutoForecast(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality, string readFrom)
        { 
            var viewModel = new AutoViewModel();
         
            string lastFolder;
            try
            {
                IEnumerable<string> assets; 
                if (readFrom.ToLower() == "assets")
                {
                    assets = _fileManager.ReadAssetsFromExcel(_directoryManager.AsstesLocation);
                }
                else
                {
                    assets = _fileManager.ReadAssetsFromExcel(_directoryManager.ObservablesLocation);
                }
                var currentTime = DateTime.Now;
                Parallel.ForEach(assets, asset =>
                    {

                        var pathToFolder =
                            _directoryManager.GenerateForecastFolder(asset, periods, DirSwitcher.Auto, currentTime);

                        var normalized = _processModel.GetDataAuto(asset, dataHours);
                        if (normalized == null || !normalized.Any())
                        {
                            _directoryManager.RemoveFolder(pathToFolder);
                            Shared.Log(asset, Indicator.ZeroRezults, 0, 0, 0);
                            return;
                        }
                        
                        var csv = _fileManager.CreateDataCsv(normalized, pathToFolder);
                        if (string.IsNullOrEmpty(csv))
                        {
                            Shared.Log(asset, Indicator.ZeroRezults, 0, 0, 0);
                            return;
                        }
                        _directoryManager.SaveDataFile(csv, pathToFolder);
                        
                        _pythonExec.RunPython(pathToFolder, periods, hourlySeasonality, dailySeasonality);

                        var pathToOut = _directoryManager.FilePathOut(pathToFolder);
                        var pathToComponents = _directoryManager.FileComponentsOut(pathToFolder);
                        var pathToForecast = _directoryManager.FileForecastOut(pathToFolder);

                        var outCreated = _directoryManager.WaitForFile(pathToOut, 60);
                        var componentsCreated = _directoryManager.WaitForFile(pathToComponents, 10);
                        var forecastCreated = _directoryManager.WaitForFile(pathToForecast, 10);

                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;

                        var stats = _fileManager.BuildOutTableRows(pathToOut, periods);
                        var performance = _utility.DefinePerformance(stats);
                        var marketFeatures = _utility.GetFeatures(normalized, asset);
                        Shared.Log(asset, performance.Indicator, performance.Rate, marketFeatures.Volume, marketFeatures.Change);
                        _directoryManager.SpecifyDirByTrend(performance.Indicator, pathToFolder);
                    }
                );

                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                var results = _directoryManager.GetListByIndicator(lastFolder);
                viewModel.NegativeAssets = results.NegativeAssets;
                viewModel.NeutralAssets = results.NeutralAssets;
                viewModel.PositiveAssets = results.PositiveAssets;
                viewModel.StrongPositiveAssets = results.StrongPositiveAssets;
                var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetLog);
                _directoryManager.WriteLogToExcel(lastFolder, res);
                Shared.ClearLog();
                viewModel.Report = _directoryManager.GetReport(lastFolder);
               
                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
            }
            catch (Exception e)
            {
                lastFolder = _directoryManager.GetLastFolder(DirSwitcher.Auto);
                if (Shared.GetLog.Any())
                {
                    var res = _fileManager.WriteLogExcel(lastFolder, Shared.GetLog);
                    _directoryManager.WriteLogToExcel(lastFolder, res);
                    Shared.ClearLog();
                }
                throw new Exception(e.Message);
            }

            return viewModel;
        }
        
        public async Task<BtcViewModel> InstantForecast()
        {
            var viewModel = new BtcViewModel();
            const int periods = 24;
            const int dataHours = 230;
            const bool hourlySeasonality = false;
            const bool dailySeasonality = false;
            var numFormat = new CultureInfo("en-US", false ).NumberFormat;
            numFormat.PercentDecimalDigits = 2;

            var settingsJson = _directoryManager.CustomSettings;
            var settings = _fileManager.ReadCustomSettings(settingsJson);
            var asset = settings.Btc;
            
            try
            {
                var normalized = _processModel.GetDataManual(asset, dataHours);
                var location = _directoryManager.GenerateForecastFolder(asset, periods, DirSwitcher.Instant);

                var csv = _fileManager.CreateDataCsv(normalized, location);
                if (string.IsNullOrEmpty(csv))
                {
                    throw new Exception("Not enough data: " + asset); 
                }
                _directoryManager.SaveDataFile(csv, location);
                
                _pythonExec.RunPython(location, periods, hourlySeasonality, dailySeasonality);
                
                var pathToOut = _directoryManager.FilePathOut(location);
                var pathToComponents = _directoryManager.FileComponentsOut(location);
                var pathToForecast = _directoryManager.FileForecastOut(location);
                
                var outCreated =  await _directoryManager.WaitForFile(pathToOut, 60);
                var componentsCreated = await _directoryManager.WaitForFile(pathToComponents, 10);
                var forecastCreated = await _directoryManager.WaitForFile(pathToForecast, 10);
                
                var images = _directoryManager.ImagePath(DirSwitcher.Instant);

                if (outCreated)
                {
                    var stats = _fileManager.BuildOutTableRows(pathToOut, periods);

                    var performance = _utility.DefinePerformance(stats);
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("P", numFormat);
                    var marketFeatures = _utility.GetFeatures(normalized, asset);
                    
                    viewModel.Volume = marketFeatures.Volume.ToString();
                    viewModel.Change = marketFeatures.Change.ToString("N2");
                }
                else 
                {
                    throw new Exception("out.csv not found");
                }
                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    throw new Exception("forecast.png not found");
                }
                
                if (!componentsCreated)
                {
                    throw new Exception("components.png not found");
                }

                var model = _requestHelper.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                viewModel.AssetName = asset;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
            return viewModel;
        } 
    }
}