using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingApp.Core.Core;
using TradingApp.Data.Enums;
using TradingApp.Data.Managers;
using TradingApp.Data.ServerRequests;
using TradingApp.Domain.Models;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string _currentLocation;

        public HomeController(IOptions<ApplicationSettings> appSettings, IHostingEnvironment env)
        {
            _appSettings = appSettings;
            _currentLocation = env.ContentRootPath;
        }
        
        [HttpGet]
        public IActionResult Index()
        { 
            var stats = new Requests();
            var model = stats.GetStats();
            return View(model);
        }

        [HttpGet]
        public IActionResult Manual()
        {
            var stats = new Requests();
            var model = stats.GetStats();
            return View(model);
        }
        
        [HttpGet]
        public IActionResult Auto()
        {
            var stats = new Requests();
            var model = stats.GetStats();
            return View(model);
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Manual(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new ManualViewModel();
            var coin = new ProcessModel(_appSettings);
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);
            var python = new PythonExec(_appSettings);
            try
            { 
                var normalized = coin.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Manual);
               
                if (!file.CreateDataCsv(normalized, location))
                {
                    return NotFound(new {message = "Not enough data: " + asset}); 
                }
                
                python.RunPython(location, periods, hourlySeasonality, dailySeasonality);
                
                var pathToOut = directory.FilePathOut(location);
                var pathToComponents = directory.FileComponentsOut(location);
                var pathToForecast = directory.FileForecastOut(location);
                
                var outCreated =  await directory.WaitForFile(pathToOut, 60);
                var componentsCreated = await directory.WaitForFile(pathToComponents, 10);
                var forecastCreated = await directory.WaitForFile(pathToForecast, 10);
                var images = directory.ImagePath(DirSwitcher.Manual);
                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    return NotFound(new { message = "forecast.png not found"});
                }
                
                if (componentsCreated)
                {
                    viewModel.ComponentsPath = images.ComponentsImage;
                }
                else
                {
                    return NotFound(new { message = "components.png not found"});
                }
                if (outCreated)
                {
                    var stats = file.BuildOutTableRows(pathToOut, periods);
                    var settingsJson = directory.CustomSettings;
                    var settings = file.ReadCustomSettings(settingsJson);
                    var utils = new Utility(settings);
                    var performance = utils.DefinePerformance(stats);
                    viewModel.Table = stats.Table;
                    viewModel.Indicator = performance.Indicator;
                }
                else 
                {
                    return NotFound(new { message = "out.csv not found" });
                }
                viewModel.AssetName = asset;
               
                var callsStats = new Requests();
                var model = callsStats.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Auto(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            
            var viewModel = new AutoViewModel();
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);

            var assets = file.ReadAssetsFromExcel(directory.AsstesLocation);
            
//            var viewModel = new AutoForecastViewModel();
//            var manager = new DirectoryManager(_appSettings, currentLocation);
//            var assets = DirectoryManager.ReadAssetsFromExcel(manager.AsstesLocation);
//            //var symbol = assets.First();
//            StaticUtility.RequestCounter = manager.GetRequestCount();//StaticUtility.AddRequestCount(manager.GetRequestCount());
//            //var pythonRun = new Logic(_appSettings);
//            var catchAsset = string.Empty;
//            try
//            {
//                Parallel.ForEach(assets, symbol =>
//                    {
//                        catchAsset = symbol;
//                        var pythonRun = new Logic(_appSettings);
//                        var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
//                        var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Auto);
//                        //coin.GenerateCsvFile(pathToFolder);
//                        if (!coin.GenerateCsvFileAuto(pathToFolder))
//                        {
//                            StaticUtility.Log(symbol, Indicator.ZeroRezults, 0);
//                            return;
//                        }
//                        pythonRun.PythonExecutor(pathToFolder, periods, hourlySeasonality, dailySeasonality);
//
//                        var pathToOut = Path.Combine(pathToFolder, manager.OutFile);
//                        var pathToComponents = Path.Combine(pathToFolder, manager.OutComponents);
//                        var pathToForecast = Path.Combine(pathToFolder, manager.OutForecast);
//
//                        var outCreated =  StaticUtility.WaitForFile(pathToOut, 20);
//                        var componentsCreated =  StaticUtility.WaitForFile(pathToComponents, 10);
//                        var forecastCreated =  StaticUtility.WaitForFile(pathToForecast, 10);
//                        if (!outCreated.Result || !forecastCreated.Result || !componentsCreated.Result) return;
//                        var table = StaticUtility.BuildOutTableRows(pathToOut, periods);
//                        var performance = coin.DefineTrend(table);
//                        StaticUtility.Log(symbol, performance.Indicator, performance.Rate);
//                        manager.SpecifyDirByTrend(performance.Indicator, pathToFolder);
//                    }
//                );
//                
//                manager.UpdateRequests(StaticUtility.RequestCounter);
//                var folder = manager.GetLastFolder(DirSwitcher.Auto);
//                StaticUtility.WriteLogExcel(folder);
//                var positiveDir = Path.Combine(folder, manager.DirPositive);
//                var neutralDir = Path.Combine(folder, manager.DirNeutral);
//                var negativeDir = Path.Combine(folder, manager.DirNegative);
//                var strongPositiveDir = Path.Combine(folder, manager.DirStrongPositive);
//                var pathToExcelLog = Path.Combine(folder, StaticUtility.LogName);  
//                if (DirectoryManager.IsFolderExist(positiveDir))
//                {
//                    viewModel.PositiveAssets = DirectoryManager.GetFolderNames(positiveDir);
//                }
//
//                if (DirectoryManager.IsFolderExist(neutralDir))
//                {
//                    viewModel.NeutralAssets = DirectoryManager.GetFolderNames(neutralDir);
//                }
//                
//                if (DirectoryManager.IsFolderExist(negativeDir))
//                {
//                    viewModel.NegativeAssets = DirectoryManager.GetFolderNames(negativeDir);
//                }
//                
//                if (DirectoryManager.IsFolderExist(strongPositiveDir))
//                {
//                    viewModel.StrongPositiveAssets = DirectoryManager.GetFolderNames(strongPositiveDir);
//                }
//
//                viewModel.RequestCount = StaticUtility.RequestCounter;
//                viewModel.Report = manager.ReadLog(pathToExcelLog);
//            }
//            catch (Exception e)
//            {
//                manager.UpdateRequests(StaticUtility.RequestCounter);
//                StaticUtility.WriteLogExcel(manager.GetLastFolder(DirSwitcher.Auto));
//                return NotFound(new {message = e.Message + " Assset: " + catchAsset, requestCount = manager.CurrentCounts});
//            }

            return Json(viewModel); 
        }
    }
}