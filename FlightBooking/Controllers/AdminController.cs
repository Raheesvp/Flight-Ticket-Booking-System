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

        [HttpGet]
        public async Task<IActionResult> Airports()
        {
            var list = await _uow.Airports.GetAllAsync();
            return View("Airport", list.OrderBy(a => a.Country)
                            .ThenBy(a => a.City));
        }

        [HttpGet]
        public IActionResult AirportAdd() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirportAdd(
            AirportViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Check duplicate IATA code
            var exists = await _uow.Airports.ExistsAsync(
                a => a.IATACode == vm.IATACode.ToUpper());
            if (exists)
            {
                ModelState.AddModelError("IATACode",
                    "An airport with this IATA code already exists.");
                return View(vm);
            }

            await _uow.Airports.AddAsync(new Airport
            {
                IATACode = vm.IATACode.ToUpper().Trim(),
                AirportName = vm.AirportName.Trim(),
                City = vm.City.Trim(),
                Country = vm.Country.Trim(),
                IsActive = vm.IsActive
            });
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Airport added successfully.";
            return RedirectToAction("Airports");
        }

        [HttpGet]
        public async Task<IActionResult> AirportEdit(int id)
        {
            var airport = await _uow.Airports.GetByIdAsync(id);
            if (airport == null) return NotFound();

            return View(new AirportViewModel
            {
                AirportId = airport.AirportId,
                IATACode = airport.IATACode,
                AirportName = airport.AirportName,
                City = airport.City,
                Country = airport.Country,
                IsActive = airport.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirportEdit(
            AirportViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var airport = await _uow.Airports
                               .GetByIdAsync(vm.AirportId);
            if (airport == null) return NotFound();

            airport.IATACode = vm.IATACode.ToUpper().Trim();
            airport.AirportName = vm.AirportName.Trim();
            airport.City = vm.City.Trim();
            airport.Country = vm.Country.Trim();
            airport.IsActive = vm.IsActive;

            _uow.Airports.Update(airport);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Airport updated.";
            return RedirectToAction("Airports");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirportToggle(
            int id)
        {
            var airport = await _uow.Airports.GetByIdAsync(id);
            if (airport == null) return NotFound();

            airport.IsActive = !airport.IsActive;
            _uow.Airports.Update(airport);
            await _uow.SaveChangesAsync();
            return RedirectToAction("Airports");
        }

        // ?? AIRLINES ???????????????????????????????????

        [HttpGet]
        public async Task<IActionResult> Airlines()
        {
            var list = await _uow.Airlines.GetAllAsync();
            return View(list.OrderBy(a => a.AirlineName));
        }

        [HttpGet]
        public IActionResult AirlineAdd() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirlineAdd(
            AirlineViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            await _uow.Airlines.AddAsync(new Airline
            {
                AirlineCode = vm.AirlineCode.ToUpper().Trim(),
                AirlineName = vm.AirlineName.Trim(),
                LogoUrl = vm.LogoUrl?.Trim(),
                IsActive = vm.IsActive
            });
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Airline added successfully.";
            return RedirectToAction("Airlines");
        }

        [HttpGet]
        public async Task<IActionResult> AirlineEdit(int id)
        {
            var airline = await _uow.Airlines.GetByIdAsync(id);
            if (airline == null) return NotFound();

            return View(new AirlineViewModel
            {
                AirlineId = airline.AirlineId,
                AirlineCode = airline.AirlineCode,
                AirlineName = airline.AirlineName,
                LogoUrl = airline.LogoUrl,
                IsActive = airline.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirlineEdit(
            AirlineViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var airline = await _uow.Airlines
                               .GetByIdAsync(vm.AirlineId);
            if (airline == null) return NotFound();

            airline.AirlineCode = vm.AirlineCode.ToUpper().Trim();
            airline.AirlineName = vm.AirlineName.Trim();
            airline.LogoUrl = vm.LogoUrl?.Trim();
            airline.IsActive = vm.IsActive;

            _uow.Airlines.Update(airline);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Airline updated.";
            return RedirectToAction("Airlines");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AirlineToggle(int id)
        {
            var airline = await _uow.Airlines.GetByIdAsync(id);
            if (airline == null) return NotFound();

            airline.IsActive = !airline.IsActive;
            _uow.Airlines.Update(airline);
            await _uow.SaveChangesAsync();
            return RedirectToAction("Airlines");
        }


    }
}
