using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingApp.Core.Core;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;

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
            IForecaster stats = new Forecaster(_appSettings, _currentLocation);
            var model = stats.GetStats();
            return View(model);
        }

        [HttpGet]
        public IActionResult Manual()
        {
            
            IForecaster stats = new Forecaster(_appSettings, _currentLocation);
            var model = stats.GetStats();
            return View(model);
        }

        [HttpGet]
        public IActionResult Auto()
        {
            IForecaster stats = new Forecaster(_appSettings, _currentLocation);
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

            try
            {
                IForecaster forecaster = new Forecaster(_appSettings, _currentLocation);
                var model = await forecaster.MakeManualForecast(asset, dataHours, periods, hourlySeasonality, dailySeasonality);
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }

        }

        [HttpPost]
        public async Task<IActionResult> Auto(int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality, string readFrom)
        {
            try
            {
                IForecaster forecaster = new Forecaster(_appSettings, _currentLocation);
                var model = await forecaster.MakeAutoForecast(dataHours, periods, hourlySeasonality, dailySeasonality, readFrom);
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }   
    }
}