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
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string _currentLocation;
        public Forecaster(IOptions<ApplicationSettings> settings, string env)
        {
            _appSettings = settings;
            _currentLocation = env;
        }

        public ServerRequestsStats GetStats()
        {
            var stats = new Requests();
            return stats.GetStats();           
        }

        public async Task<ManualViewModel> MakeManualForecast(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new ManualViewModel();
            IProcessModel coin = new ProcessModel(_appSettings);
            IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
            IFileManager file = new FileManager(_appSettings);
            IPythonExec python = new PythonExec(_appSettings);
            try
            {
                var normalized = coin.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Manual);
                
                if (!file.CreateDataCsv(normalized, location))
                {
                    throw new Exception("Not enough data: " + asset);
                }

                python.RunPython(location, periods, hourlySeasonality, dailySeasonality);

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
                    var settingsJson = directory.CustomSettings;
                    var settings = file.ReadCustomSettings(settingsJson);
                    IUtility utils = new Utility(settings);
                    var performance = utils.DefinePerformance(stats);
                    viewModel.Table = stats.Table;
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("N2");
                    var marketFeatures = utils.GetFeatures(normalized, asset);
                    
                    viewModel.Volume = marketFeatures.Volume.ToString();
                    viewModel.Change = marketFeatures.Change.ToString("N2");
                }
                else
                {
                    throw new Exception("out.csv not found");
                }

                viewModel.AssetName = asset;

                IRequests callsStats = new Requests();
                var model = callsStats.GetStats();
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
            IDirectoryManager getAssets = new DirectoryManager(_appSettings, _currentLocation);
            IFileManager readAssets = new FileManager(_appSettings);
            IFileManager log = new FileManager(_appSettings);
            IDirectoryManager folder = new DirectoryManager(_appSettings, _currentLocation);
            string lastFolder;
            try
            {
                IEnumerable<string> assets; 
                if (readFrom.ToLower() == "assets")
                {
                    assets = readAssets.ReadAssetsFromExcel(getAssets.AsstesLocation);
                }
                else
                {
                    assets = readAssets.ReadAssetsFromExcel(getAssets.ObservablesLocation);
                }

                var currentTime = DateTime.Now;
                

                Parallel.ForEach(assets, asset =>
                    {
                        IProcessModel coin = new ProcessModel(_appSettings);
                        IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
                        IFileManager file = new FileManager(_appSettings);
                        IPythonExec python = new PythonExec(_appSettings);
                        var pathToFolder =
                            directory.GenerateForecastFolder(asset, periods, DirSwitcher.Auto, currentTime);

                        var normalized = coin.GetDataAuto(asset, dataHours);
                        if (normalized == null || !normalized.Any())
                        {
                            directory.RemoveFolder(pathToFolder);
                            Shared.Log(asset, Indicator.ZeroRezults, 0, 0, 0);
                            return;
                        }

                        if (!file.CreateDataCsv(normalized, pathToFolder))
                        {
                            Shared.Log(asset, Indicator.ZeroRezults, 0, 0, 0);
                            return;
                        }

                        python.RunPython(pathToFolder, periods, hourlySeasonality, dailySeasonality);

                        var pathToOut = directory.FilePathOut(pathToFolder);
                        var pathToComponents = directory.FileComponentsOut(pathToFolder);
                        var pathToForecast = directory.FileForecastOut(pathToFolder);

                        var outCreated = directory.WaitForFile(pathToOut, 60);
                        var componentsCreated = directory.WaitForFile(pathToComponents, 10);
                        var forecastCreated = directory.WaitForFile(pathToForecast, 10);

                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;

                        var stats = file.BuildOutTableRows(pathToOut, periods);
                        var settingsJson = directory.CustomSettings;
                        var settings = file.ReadCustomSettings(settingsJson);
                        IUtility utils = new Utility(settings);
                        var performance = utils.DefinePerformance(stats);
                        var marketFeatures = utils.GetFeatures(normalized, asset);
                        Shared.Log(asset, performance.Indicator, performance.Rate, marketFeatures.Volume, marketFeatures.Change);
                        directory.SpecifyDirByTrend(performance.Indicator, pathToFolder);
                        
                    }
                );

                lastFolder = folder.GetLastFolder(DirSwitcher.Auto);
                var results = folder.GetListByIndicator(lastFolder);
                IDirectoryManager manager = new DirectoryManager(_appSettings, _currentLocation);
                viewModel.NegativeAssets = results.NegativeAssets;
                viewModel.NeutralAssets = results.NeutralAssets;
                viewModel.PositiveAssets = results.PositiveAssets;
                viewModel.StrongPositiveAssets = results.StrongPositiveAssets;
                log.WriteLogExcel(lastFolder, Shared.GetLog);
                Shared.ClearLog();
                viewModel.Report = manager.GetReport(lastFolder);
                IRequests callsStats = new Requests();
                var model = callsStats.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
            }
            catch (Exception e)
            {
                lastFolder = folder.GetLastFolder(DirSwitcher.Auto);
                if (Shared.GetLog.Any())
                {
                    log.WriteLogExcel(lastFolder, Shared.GetLog);
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
            
            IProcessModel coin = new ProcessModel(_appSettings);
            IDirectoryManager directory = new DirectoryManager(_appSettings, _currentLocation);
            IFileManager file = new FileManager(_appSettings);
            IPythonExec python = new PythonExec(_appSettings);
            var settingsJson = directory.CustomSettings;
            var settings = file.ReadCustomSettings(settingsJson);
            var asset = settings.Btc;
            
            try
            {
                var normalized = coin.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Instant);
               
                if (!file.CreateDataCsv(normalized, location))
                {
                    throw new Exception("Not enough data: " + asset); 
                }
                python.RunPython(location, periods, hourlySeasonality, dailySeasonality);
                
                var pathToOut = directory.FilePathOut(location);
                var pathToComponents = directory.FileComponentsOut(location);
                var pathToForecast = directory.FileForecastOut(location);
                
                var outCreated =  await directory.WaitForFile(pathToOut, 60);
                var componentsCreated = await directory.WaitForFile(pathToComponents, 10);
                var forecastCreated = await directory.WaitForFile(pathToForecast, 10);
                
                var images = directory.ImagePath(DirSwitcher.Instant);

                if (outCreated)
                {
                    var stats = file.BuildOutTableRows(pathToOut, periods);
                    IUtility utils = new Utility(settings);
                    var performance = utils.DefinePerformance(stats);
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("P", numFormat);
                    var marketFeatures = utils.GetFeatures(normalized, asset);
                    
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
                IRequests callsStats = new Requests();
                var model = callsStats.GetStats();
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