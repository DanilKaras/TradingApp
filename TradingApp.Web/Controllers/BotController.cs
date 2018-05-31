using System;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Core.BotTools;

namespace TradingApp.Web.Controllers
{
    public class BotController : Controller
    {
        private readonly IFireScheduler _scheduler;
        public BotController(IFireScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        
        
        [HttpGet]
        public IActionResult BotPortal()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FireTask()
        {
            try
            {
                _scheduler.FireForecaster();
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            return Ok();
        }
        
        [HttpPost]
        public IActionResult TriggerTask()
        {
            try
            {
                _scheduler.TriggerImmediately();
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            return Ok();
        }
        
        [HttpPost]
        public IActionResult DeleteTask()
        {
            try
            {
                _scheduler.StopForecaster();
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            return Ok();
        }
        [HttpPost]
        public IActionResult FireTaskSecond()
        {
            try
            {
                _scheduler.FireForecasterSecond();
            }
            catch (Exception e)
            {
                return NotFound(new {message = e.Message});
            }
            return Ok();
        }
    }
}