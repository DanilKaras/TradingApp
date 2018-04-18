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
    public class DataController : Controller
    {
        // GET
        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string _currentLocation;

        
        public DataController(IOptions<ApplicationSettings> appSettings, IHostingEnvironment env)
        {
            _appSettings = appSettings;
            _currentLocation = env.ContentRootPath;
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
        
        public async Task<IActionResult> InstantForecast()
        {
            
            
            var viewModel = new BtcViewModel();
            const int periods = 24;
            const int dataHours = 230;
            const bool hourlySeasonality = false;
            const bool dailySeasonality = false;
            var numFormat = new CultureInfo("en-US", false ).NumberFormat;
            numFormat.PercentDecimalDigits = 3;
            
            var coin = new ProcessModel(_appSettings);
            var directory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);
            var python = new PythonExec(_appSettings);
            var settingsJson = directory.CustomSettings;
            var settings = file.ReadCustomSettings(settingsJson);
            var asset = settings.Btc;
            
            try
            {
                var normalized = coin.GetDataManual(asset, dataHours);
                var location = directory.GenerateForecastFolder(asset, periods, DirSwitcher.Instant);
               
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
                
                var images = directory.ImagePath(DirSwitcher.Instant);

                if (outCreated)
                {
                    var stats = file.BuildOutTableRows(pathToOut, periods);
                    var utils = new Utility(settings);
                    var performance = utils.DefinePerformance(stats);
                    viewModel.Indicator = performance.Indicator;
                    viewModel.Rate = performance.Rate.ToString("P", numFormat);
                }
                else 
                {
                    return NotFound(new { message = "out.csv not found" });
                }
                if (forecastCreated)
                {
                    viewModel.ForecastPath = images.ForecastImage;
                }
                else
                {
                    return NotFound(new { message = "forecast.png not found"});
                }
                
                if (!componentsCreated)
                {
                    return NotFound(new { message = "components.png not found" });
                }
                var callsStats = new Requests();
                var model = callsStats.GetStats();
                viewModel.CallsLeftHisto = model.CallsLeft.Histo;
                viewModel.CallsMadeHisto = model.CallsMade.Histo;
                viewModel.AssetName = asset;

            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            return Json(viewModel);
        }
    }
}