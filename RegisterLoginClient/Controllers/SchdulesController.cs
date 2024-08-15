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

        public IActionResult ScduleDetials(int scheduleId)
        {
            ViewBag.scheduleId = scheduleId;
            return View(scheduleId);
        }
    }
}