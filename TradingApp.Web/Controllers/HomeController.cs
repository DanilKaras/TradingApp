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
        private readonly IForecaster _forecaster;
        public HomeController(IForecaster forecaster)
        {
            _forecaster = forecaster;
        }

        [HttpGet]
        public IActionResult Index()
        {           
            var model = _forecaster.GetStats();
            return View(model);
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
    }
}