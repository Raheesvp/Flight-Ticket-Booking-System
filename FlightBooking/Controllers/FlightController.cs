using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Controllers
{
    public class FlightController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
