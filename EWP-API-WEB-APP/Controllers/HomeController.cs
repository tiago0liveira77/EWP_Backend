using EWP_API_WEB_APP.Data;
using EWP_API_WEB_APP.Models;
using EWP_API_WEB_APP.Models.API.Requests;
using EWP_API_WEB_APP.Models.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EWP_API_WEB_APP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Landing page (Página inicial)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            _logger.LogInformation("HomeController: Index action executed.");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("HomeController: Privacy action executed.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("HomeController: Error action executed.");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
