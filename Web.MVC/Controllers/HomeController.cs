using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Web.MVC.Helper;
using Web.MVC.Models;

namespace Web.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceHelper _serviceProvider;

        public HomeController(ILogger<HomeController> logger, IServiceHelper serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.OrderData = await _serviceProvider.GetOrder();
            ViewBag.ProductData = await _serviceProvider.GetProduct();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}