using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.Extensions.Options;
using TradingApp.Data.Enums;
using TradingApp.Data.Managers;
using TradingApp.Data.Models;
using TradingApp.Data.ServerRequests;
using TradingApp.Domain.Core;
using TradingApp.Web.Models;

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
            return View();
        }
        
        [HttpGet]
        public IActionResult Auto()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult LoadExchanges()
        {
            var viewModel = new SettingsViewModel();
            var exchanges = new Requests();
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);
            var settingsJson = directory.CustomSettings;
            var settings = file.ReadCustomSettings(settingsJson);
            

            viewModel.Exchanges = exchanges.GetExchanges();
            viewModel.Btc = settings.Btc;
            viewModel.LastExchange = settings.Exchange;
            viewModel.LowerBorder = settings.LowerBorder;
            viewModel.UpperBorder = settings.UpperBorder;
            
            return Json(viewModel);
        }
   
        public IActionResult UpdateExchanges(SettingsViewModel settings)
        {  
            var newSettings = new CustomSettings();
            var update = new Requests();
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            
            var file = new FileManager(_appSettings);
            
            try
            {
                var model = update.GetAssets(settings.LastExchange);
                file.WriteAssetsToExcel(directory.AsstesUpdateLocation, model);
                newSettings.Btc = model.Btc;
                newSettings.Exchange = model.ExchangeName;
                newSettings.LowerBorder = settings.LowerBorder;
                newSettings.UpperBorder = settings.UpperBorder;

                var json = file.ConvertCustomSettings(newSettings);
                directory.UpdateCustomSettings(json);
                
                return Json(newSettings); 
            }
            catch (Exception e)
            {
                return NotFound(new {e.Message});
            }
        }

        public IActionResult GetAssets()
        {
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);

            var assets = file.ReadAssetsFromExcel(directory.AsstesLocation);

            return Json(assets);
        }
        
        [HttpPost]
        public async Task<IActionResult> Manual(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            var viewModel = new ManualViewModel();
            var coin = new ProcessModel(_appSettings);
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);
            try
            {
                var normalized = coin.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Manual);
                if (!file.CreateDataCsv(normalized, location))
                {
                    return NotFound(new {message = "Not enough data: " + asset});
                }
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
           

            
            
            //LoadToCsv
//            var request = new Requests();
//            var model = request.GetCoinData(asset);
//            var coin = new Logic(_appSettings, symbol, dataHours, currentLocation);
//            var manager = new DirectoryManager(_appSettings, currentLocation);
//            var pythonRun = new Logic(_appSettings);
//            StaticUtility.RequestCounter = (manager.GetRequestCount());
//            try
//            {
//                var pathToFolder = manager.GenerateForecastFolder(symbol, periods, DirSwitcher.Manual);
//                if (!coin.GenerateCsvFile(pathToFolder))
//                {
//                    throw new Exception("Something's wrong with a coin");
//                }
//                
//                pythonRun.PythonExecutor(manager.GetLastFolder(DirSwitcher.Manual), periods, hourlySeasonality, dailySeasonality);
//
//                var pathToOut = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutFile);
//                var pathToComponents = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutComponents);
//                var pathToForecast = Path.Combine(manager.GetLastFolder(DirSwitcher.Manual), manager.OutForecast);
//               
//                var pathToComponentsForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Manual), manager.OutComponents);
//                var pathToForecastForImg = Path.Combine(_appSettings.Value.ForecastDir, manager.DirForImages(DirSwitcher.Manual), manager.OutForecast);
//                
//                var outCreated =  await StaticUtility.WaitForFile(pathToOut, 60);
//                var componentsCreated = await StaticUtility.WaitForFile(pathToComponents, 10);
//                var forecastCreated = await StaticUtility.WaitForFile(pathToForecast, 10);
//
//                if (outCreated)
//                {
//                    viewModel.Table = StaticUtility.BuildOutTableRows(pathToOut, periods);
//                }
//                else 
//                {
//                    manager.UpdateRequests(StaticUtility.RequestCounter);
//                    return NotFound(new { message = "out.csv not found", requestCount = StaticUtility.RequestCounter });
//                }
//
//                if (forecastCreated)
//                {
//                    viewModel.ForecastPath = Path.DirectorySeparatorChar + pathToForecastForImg;
//                }
//                else
//                {
//                    manager.UpdateRequests(StaticUtility.RequestCounter);
//                    return NotFound(new { message = "forecast.png not found", requestCount = StaticUtility.RequestCounter});
//                }
//                
//                if (componentsCreated)
//                {
//                    viewModel.ComponentsPath = Path.DirectorySeparatorChar + pathToComponentsForImg;
//                }
//                else
//                {
//                    manager.UpdateRequests(StaticUtility.RequestCounter);
//                    return NotFound(new { message = "components.png not found", requestCount = StaticUtility.RequestCounter});
//                }
//                
//                manager.UpdateRequests(StaticUtility.RequestCounter);
//                viewModel.RequestsPerDay = StaticUtility.RequestCounter;//manager.CurrentCounts;
//                viewModel.AssetName = symbol;
//                var performance = coin.DefineTrend(viewModel.Table);
//                viewModel.Indicator = performance.Indicator;
//            }
//            catch (Exception e)
//            {
//                manager.UpdateRequests(StaticUtility.RequestCounter);
//                return NotFound(new {message = e.Message, requestCount = StaticUtility.RequestCounter});
//            }
//            
            return Json(null);
        }
    }
}