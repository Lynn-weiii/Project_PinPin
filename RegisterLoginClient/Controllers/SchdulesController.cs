using Microsoft.AspNetCore.Mvc;

namespace PinPinClient.Controllers
{
    public class SchdulesController : Controller
    {
        //GET:Schdules/Index
        public IActionResult Index()
        {
            return View();
        }


    }
}