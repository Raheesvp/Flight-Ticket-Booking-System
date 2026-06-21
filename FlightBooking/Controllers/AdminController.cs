using FlightBooking.Data;
using FlightBooking.Models.Domain;
using FlightBooking.Models.ViewModels;
using FlightBooking.Web.Data;
using FlightBooking.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public AdminController(IUnitOfWork uow, UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _uow = uow;
            _userManager = userManager;
            _db = db;
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

        // ?? FLIGHTS ????????????????????????????????????

        private async Task PopulateFlightDropdowns(
            FlightViewModel vm)
        {
            var airlines = await _uow.Airlines
                .FindAsync(a => a.IsActive);
            var airports = await _uow.Airports
                .FindAsync(a => a.IsActive);

            vm.Airlines = airlines
                .OrderBy(a => a.AirlineName)
                .Select(a => new SelectListItem
                {
                    Value = a.AirlineId.ToString(),
                    Text = $"{a.AirlineCode} — {a.AirlineName}"
                });

            vm.Airports = airports
                .OrderBy(a => a.City)
                .Select(a => new SelectListItem
                {
                    Value = a.AirportId.ToString(),
                    Text = $"{a.IATACode} — {a.City}"
                });
        }

        [HttpGet]
        public async Task<IActionResult> Flights()
        {
            var flights = await _uow.Flights.GetAllAsync();
            return View(flights
                .OrderBy(f => f.DepartureTime));
        }

        [HttpGet]
        public async Task<IActionResult> FlightAdd()
        {
            var vm = new FlightViewModel
            {
                DepartureTime = DateTime.Today.AddHours(8),
                ArrivalTime = DateTime.Today.AddHours(10),
                TotalSeats = 180,
                Status = "Scheduled"
            };
            await PopulateFlightDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlightAdd(
            FlightViewModel vm)
        {
            if (vm.ArrivalTime <= vm.DepartureTime)
                ModelState.AddModelError("ArrivalTime",
                    "Arrival must be after departure.");

            if (vm.FromAirportId == vm.ToAirportId)
                ModelState.AddModelError("ToAirportId",
                    "From and To airports cannot be the same.");

            if (!ModelState.IsValid)
            {
                await PopulateFlightDropdowns(vm);
                return View(vm);
            }

            await _uow.Flights.AddAsync(new Flight
            {
                FlightNumber = vm.FlightNumber.ToUpper().Trim(),
                AirlineId = vm.AirlineId,
                FromAirportId = vm.FromAirportId,
                ToAirportId = vm.ToAirportId,
                DepartureTime = vm.DepartureTime,
                ArrivalTime = vm.ArrivalTime,
                DurationMinutes = vm.DurationMinutes,
                AircraftType = vm.AircraftType?.Trim(),
                TotalSeats = vm.TotalSeats,
                AvailableSeats = vm.TotalSeats,  // start = total
                BasePrice = vm.BasePrice,
                Status = vm.Status
            });
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Flight added.";
            return RedirectToAction("Flights");
        }

        [HttpGet]
        public async Task<IActionResult> FlightEdit(int id)
        {
            var f = await _uow.Flights.GetByIdAsync(id);
            if (f == null) return NotFound();

            var vm = new FlightViewModel
            {
                FlightId = f.FlightId,
                FlightNumber = f.FlightNumber,
                AirlineId = f.AirlineId,
                FromAirportId = f.FromAirportId,
                ToAirportId = f.ToAirportId,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                DurationMinutes = f.DurationMinutes,
                AircraftType = f.AircraftType,
                TotalSeats = f.TotalSeats,
                BasePrice = f.BasePrice,
                Status = f.Status
            };
            await PopulateFlightDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlightEdit(
            FlightViewModel vm)
        {
            if (vm.ArrivalTime <= vm.DepartureTime)
                ModelState.AddModelError("ArrivalTime",
                    "Arrival must be after departure.");

            if (vm.FromAirportId == vm.ToAirportId)
                ModelState.AddModelError("ToAirportId",
                    "From and To airports cannot be same.");

            if (!ModelState.IsValid)
            {
                await PopulateFlightDropdowns(vm);
                return View(vm);
            }

            var f = await _uow.Flights.GetByIdAsync(vm.FlightId);
            if (f == null) return NotFound();

            f.FlightNumber = vm.FlightNumber.ToUpper().Trim();
            f.AirlineId = vm.AirlineId;
            f.FromAirportId = vm.FromAirportId;
            f.ToAirportId = vm.ToAirportId;
            f.DepartureTime = vm.DepartureTime;
            f.ArrivalTime = vm.ArrivalTime;
            f.DurationMinutes = vm.DurationMinutes;
            f.AircraftType = vm.AircraftType?.Trim();
            f.TotalSeats = vm.TotalSeats;
            f.BasePrice = vm.BasePrice;
            f.Status = vm.Status;

            _uow.Flights.Update(f);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Flight updated.";
            return RedirectToAction("Flights");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlightStatusChange(
            int id, string status)
        {
            var f = await _uow.Flights.GetByIdAsync(id);
            if (f == null) return NotFound();

            f.Status = status; // Scheduled|Delayed|Cancelled
            _uow.Flights.Update(f);
            await _uow.SaveChangesAsync();
            return RedirectToAction("Flights");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel();

            // 1. Calculate High-Level Structural KPI Card Statistics
            viewModel.TotalRevenue = await _db.Bookings.SumAsync(b => b.TotalAmount);
            viewModel.TotalBookingsCount = await _db.Bookings.CountAsync();
            viewModel.ActiveFlightsCount = await _db.Flights.CountAsync(f => f.DepartureTime >= DateTime.UtcNow);
            viewModel.TotalRegisteredPassengers = await _db.Passengers.CountAsync();

            // 2. Fetch Monthly Revenue Analytics (Past 6 Months) for Line Graph
            var baselineDate = DateTime.UtcNow.AddMonths(-5);
            var monthlyData = await _db.Bookings
                .Where(b => b.BookingDate >= new DateTime(baselineDate.Year, baselineDate.Month, 1))
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthlySum = g.Sum(b => b.TotalAmount)
                })
                .ToListAsync();

            // Sort and project chronological order onto the view model vectors
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = DateTime.UtcNow.AddMonths(-i);
                var match = monthlyData.FirstOrDefault(m => m.Year == targetMonth.Year && m.Month == targetMonth.Month);

                viewModel.RevenueLabels.Add(targetMonth.ToString("MMM yyyy"));
                viewModel.RevenueDataPoints.Add(match?.MonthlySum ?? 0);
            }

            // 3. Fetch Market Share Breakdown (Bookings per Airline) for Doughnut Chart
            var airlineShare = await _db.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .GroupBy(b => b.Flight.Airline.AirlineName)
                .Select(g => new
                {
                    AirlineName = g.Key ?? "Unknown Provider",
                    Count = g.Count()
                })
                .Take(5) // Limit to top 5 running carriers
                .ToListAsync();

            foreach (var record in airlineShare)
            {
                viewModel.AirlineLabels.Add(record.AirlineName);
                viewModel.AirlineBookingCounts.Add(record.Count);
            }

            return View(viewModel);


        }

        [HttpGet]
        public async Task<IActionResult> Bookings(AdminBookingFilterViewModel filters)
        {
            // Build out base dynamic querying tree with explicit eager loading configurations
            IQueryable<Booking> query = _db.Bookings
                .Include(b => b.Passengers)
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport);

            // 1. Evaluate and append optional multi-parameter filter matrices
            if (!string.IsNullOrWhiteSpace(filters.SearchPnr))
            {
                query = query.Where(b => b.PNR.Contains(filters.SearchPnr.Trim().ToUpper()));
            }

            if (filters.FilterAirlineId.HasValue)
            {
                query = query.Where(b => b.Flight.AirlineId == filters.FilterAirlineId.Value);
            }

            if (filters.FilterDate.HasValue)
            {
                query = query.Where(b => b.BookingDate.Date == filters.FilterDate.Value.Date);
            }

            // 2. Compute Pagination bounds via non-blocking asynchronous count sweeps
            int totalBookings = await query.CountAsync();
            int skipRecords = (filters.Page - 1) * filters.PageSize;

            var paginatedResults = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip(skipRecords)
                .Take(filters.PageSize)
                .ToListAsync();

            // 3. Populate lookup list dependencies for select dropdown components
            var airlines = await _db.Airlines.OrderBy(a => a.AirlineName).ToListAsync();
            var airlineOptions = airlines.Select(a => new SelectListItem
            {
                Value = a.AirlineId.ToString(),
                Text = a.AirlineName,
                Selected = filters.FilterAirlineId == a.AirlineId
            }).ToList();

            // 4. Bind compiled metadata vectors directly back onto presentation container
            var indexVm = new AdminBookingIndexViewModel
            {
                Bookings = paginatedResults,
                Filters = filters,
                AirlinesList = airlineOptions,
                TotalItems = totalBookings,
                CurrentPage = filters.Page,
                TotalPages = (int)Math.Ceiling((double)totalBookings / filters.PageSize)
            };

            return View(indexVm);
        }
    }

}
