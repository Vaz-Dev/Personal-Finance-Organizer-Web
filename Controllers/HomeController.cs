using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PFO_Web.Models;
using PFO_Web.Services;

namespace PFO_Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly DataService _dataService = new();

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var data = _dataService.Load();
            ViewBag.Data = data;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
