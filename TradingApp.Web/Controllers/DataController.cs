using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TradingApp.Core.Core;
using TradingApp.Domain.Enums;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Models;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Web.Controllers
{
    public class DataController : Controller
    {
        private readonly IHelpers _helpers;
        private readonly IForecaster _forecaster;
        
        public DataController(IHelpers helpers, IForecaster forecaster)
        {
            _helpers = helpers;
            _forecaster = forecaster;
        }

        public IActionResult LoadExchanges()
        {
            try
            {
                var viewModel = _helpers.LoadExchanges();
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {e.Message});
            }
        }

        public IActionResult UpdateExchanges(SettingsViewModel settings)
        {
            try
            {
                var viewModel = _helpers.UpdateExchanges(settings);
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {e.Message});
            }
        }

        public IActionResult GetAssets()
        {
            try
            {

                var viewModel = _helpers.GetAssets();
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }

        public IActionResult GetLatestAssets()
        {
            try
            {
                var viewModel = _helpers.GetLatestAssets();
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }

        public IActionResult GetForecastData(Indicator indicator, string assetName, int periods)
        {
            try
            {
                var viewModel = _helpers.GetForecastData(indicator, assetName, periods);
                return Json(viewModel);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }

        public IActionResult UpdateObservable(List<string> observableList)
        {
            try
            {
                _helpers.WriteObservables(observableList);
                return Ok();
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            
        }
        
        public async Task<IActionResult> InstantForecast()
        {
            try
            {
                var model = await _forecaster.InstantForecast();
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }
    }
}