using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using TradingApp.Domain.ViewModels;

namespace TradingApp.Web.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menu = new NavigationViewModel();
            menu.Navigation = new List<NavigationItem>()
            {               
                new NavigationItem
                {
                    Action = "Index",
                    Controller = "Home",
                    Text = "Settings"
                },
                new NavigationItem
                {
                    Action = "Auto",
                    Controller = "Home",
                    Text = "Auto Forecast"
                },
                new NavigationItem
                {
                    Action = "Manual",
                    Controller = "Home",
                    Text = "Manual Forecast"
                }
            };
            return View("Navigation", menu);
           
        }
    }
}