using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingApp.Data.Managers;
using TradingApp.Data.Models;
using TradingApp.Data.ServerRequests;
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
            return View();
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
            var viewModel = new SettingsViewModel();
            var newSettings = new CustomSettings();
            var update = new Requests();
            var direcory = new DirectoryManager(_appSettings, _currentLocation);
            var file = new FileManager(_appSettings);
            
            try
            {
                var model = update.GetAssets(settings.LastExchange);
                file.WriteAssetsToExcel(direcory.AsstesLocation, model);
                newSettings.Btc = model.Btc;
                newSettings.Exchange = model.ExchangeName;
                newSettings.LowerBorder = settings.LowerBorder;
                newSettings.UpperBorder = settings.UpperBorder;

                var json = file.ConvertCustomSettings(newSettings);
                direcory.UpdateCustomSettings(json);
                
                return Json(newSettings); 
            }
            catch (Exception e)
            {
                return NotFound(new {e.Message});
            }
        }
    }
}