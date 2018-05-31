using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IForecaster _forecaster;
        private readonly ILogger _logger;
        public HomeController(IForecaster forecaster, ILoggerFactory logger)
        {
            _forecaster = forecaster;
            _logger = logger.CreateLogger("Controller.HomeController");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Manual()
        {                       
            var model = _forecaster.GetStats();
            return View(model);
        }

        [HttpGet]
        public IActionResult Auto()
        {
            
            var model = _forecaster.GetStats();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manual(string asset, int dataHours, int periods, bool hourlySeasonality, bool dailySeasonality)
        {
            try
            {               
                var model = await _forecaster.MakeManualForecast(asset, dataHours, periods, hourlySeasonality, dailySeasonality);
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
                var model = await _forecaster.MakeAutoForecast(dataHours, periods, hourlySeasonality, dailySeasonality, readFrom);
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }

        [HttpGet]
        public IActionResult BotForecast()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> BotForecast(int rsi, List<int> trend, List<int> border)
        {
            try
            {
                var model = await _forecaster.MakeBotForecast(rsi, trend, border);
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }
    }
}