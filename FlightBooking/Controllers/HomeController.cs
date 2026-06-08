using FlightBooking.Data;
using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _uow;

        public HomeController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            var airports = await _uow.Airports.GetAllAsync();

            // Build SelectList for From / To dropdowns
            ViewBag.Airports = new SelectList(
                airports,
                "AirportId",
                "IATACode"       // shown to user: COK, DEL
            );

            return View(airports);
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult NotFound404()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}