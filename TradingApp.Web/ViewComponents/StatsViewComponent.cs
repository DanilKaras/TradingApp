using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Web.ViewComponents
{
    public class StatsViewComponent : ViewComponent
    {
        private readonly IForecaster _forecaster;
        public StatsViewComponent(IForecaster forecaster)
        {
            _forecaster = forecaster;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = _forecaster.GetStats();
            return View("Stats", model);
        }
    }
}