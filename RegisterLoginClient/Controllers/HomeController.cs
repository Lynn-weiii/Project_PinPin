using Microsoft.AspNetCore.Mvc;
using PinPinClient.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PinPinClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult MemberInfo()
        {
            return View();
        }

        //public IActionResult MemberInfo(ClaimsPrincipal user)
        public IActionResult Schdules() //by bao
        {
            return View();
        }

        public IActionResult AddSchmodal() //by bao
        {
            return PartialView();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
