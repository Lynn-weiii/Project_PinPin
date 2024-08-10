using Microsoft.AspNetCore.Mvc;

namespace PinPinClient.Controllers
{
    public class ChatRoomController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
