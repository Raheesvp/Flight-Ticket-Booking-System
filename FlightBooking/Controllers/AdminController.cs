using FlightBooking.Data;
using FlightBooking.Models.Domain;
using FlightBooking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _uow.Bookings.GetAllAsync();
            var flights = await _uow.Flights.GetAllAsync();
            var airports = await _uow.Airports.GetAllAsync();
            var today = DateTime.Today;

            var vm = new AdminDashboardViewModel
            {
                TotalBookings = bookings.Count(),
                BookingsToday = bookings
                    .Count(b => b.BookingDate.Date == today),
                TotalRevenue = bookings
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.TotalAmount),
                TotalFlights = flights.Count(),
                TotalAirports = airports.Count(),
                TotalUsers = _userManager.Users.Count(),
                ConfirmedBookings = bookings
                    .Count(b => b.Status == "Confirmed"),
                CancelledBookings = bookings
                    .Count(b => b.Status == "Cancelled"),
                PendingBookings = bookings
                    .Count(b => b.Status == "Pending")
            };

            return View(vm);

        }

        public IActionResult Flights() => View();
        public IActionResult Bookings() => View();
        public IActionResult Users() => View();
        public IActionResult Airlines() => View();


    }
}
