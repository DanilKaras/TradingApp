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

        private readonly IOptions<ApplicationSettings> _appSettings;
        private readonly string _currentLocation;

        public DataController(IOptions<ApplicationSettings> appSettings, IHostingEnvironment env)
        {
            _appSettings = appSettings;
            _currentLocation = env.ContentRootPath;
        }

        public IActionResult LoadExchanges()
        {
            try
            {
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                var viewModel = helper.LoadExchanges();
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
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                var viewModel = helper.UpdateExchanges(settings);
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
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                var viewModel = helper.GetAssets();
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
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                var viewModel = helper.GetLatestAssets();
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
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                var viewModel = helper.GetForecastData(indicator, assetName, periods);
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
                IHelpers helper = new Helpers(_appSettings, _currentLocation);
                helper.WriteObservables(observableList);
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
                IForecaster forecaster = new Forecaster(_appSettings, _currentLocation);
                var model = await forecaster.InstantForecast();
                return Json(model);
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
        }
    }
}