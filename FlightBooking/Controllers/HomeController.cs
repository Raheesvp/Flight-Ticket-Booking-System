using FlightBooking.Data;
using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // Read all airports from DB via repo
            var airports = await _unitOfWork.Airports.GetAllAsync();
            return View(airports);
        }
    }
}