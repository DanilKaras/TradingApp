using Microsoft.AspNetCore.Mvc;

namespace TradingApp.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}