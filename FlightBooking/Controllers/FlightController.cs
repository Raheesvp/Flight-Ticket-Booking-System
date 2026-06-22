using FlightBooking.Models.ViewModels;
using FlightBooking.Services;
using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Controllers
{
    public class FlightController : Controller
    {

        private readonly AppDbContext _dbcontext;
        private readonly SeatService _seatService;
        private readonly ICacheService _cacheService;

        public FlightController(AppDbContext dbcontext, SeatService seatService,ICacheService cacheService)
        {
            _dbcontext = dbcontext;
            _seatService = seatService;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            string cacheKey = $"flights_search_{model.FromAirportId}_{model.ToAirportId}_{model.DepartureDate:yyyyMMdd}_{model.Passengers}_{model.JourneyClass}";

            var cachedFlights = await _cacheService.GetAsync<List<FlightSearchResultViewModel>>(cacheKey);

            if (cachedFlights != null)
            {
                // Cache Hit! Return serialized view models instantly bypassing SQL engine loops entirely
                ViewBag.IsCachedData = true;
                ViewBag.SearchModel = model;
                ViewBag.ResultCount = cachedFlights.Count;
                ViewBag.FromAirportName = cachedFlights.FirstOrDefault()?.FromCity ?? "";
                ViewBag.ToAirportName = cachedFlights.FirstOrDefault()?.ToCity ?? "";
                return View(cachedFlights);
            }

            // Validate route
            if (model.FromAirportId == model.ToAirportId)
            {
                TempData["Error"] =
                    "From and To airports must differ.";
                return RedirectToAction(
                    "Index", "Home");
            }

            // Base query — eager load navigations
            var query = _dbcontext.Flights
                .Include(f => f.Airline)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .Where(f =>
                    f.Status == "Scheduled" &&
                    f.FromAirportId == model.FromAirportId &&
                    f.ToAirportId == model.ToAirportId &&
                    f.DepartureTime.Date == model.DepartureDate.Date &&
                    f.AvailableSeats >= model.Passengers)
                .OrderBy(f => f.BasePrice);

            var flights = await query.ToListAsync();

            // Map to result ViewModels
            var results = flights.Select(f =>
                new FlightSearchResultViewModel
                {
                    FlightId = f.FlightId,
                    FlightNumber = f.FlightNumber,
                    AirlineName = f.Airline.AirlineName,
                    AirlineLogoUrl = f.Airline.LogoUrl,
                    FromIATA = f.FromAirport.IATACode,
                    FromCity = f.FromAirport.City,
                    ToIATA = f.ToAirport.IATACode,
                    ToCity = f.ToAirport.City,
                    DepartureTime = f.DepartureTime,
                    ArrivalTime = f.ArrivalTime,
                    DurationMinutes = f.DurationMinutes,
                    AvailableSeats = f.AvailableSeats,
                    BasePrice = f.BasePrice,
                    TotalPrice = f.BasePrice * model.Passengers,
                    AircraftType = f.AircraftType ?? "—"
                }).ToList();

            // Pass search params back for display
            ViewBag.SearchModel = model;
            ViewBag.ResultCount = results.Count;
            ViewBag.FromAirportName = flights.FirstOrDefault()
                ?.FromAirport?.City ?? "";
            ViewBag.ToAirportName = flights.FirstOrDefault()
                ?.ToAirport?.City ?? "";

            // 3. Save to Redis memory with an absolute TTL window of 5 minutes to keep results reasonably fresh
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5));
            ViewBag.IsCachedData = false;

            return View(results);

        }

        [HttpGet]
        public async Task<IActionResult> SeatMap(int flightId, int passengers, string journeyClass)
        {
            var flight = await _dbcontext.Flights
                .Include(f => f.Airline)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null)
            {
                return NotFound();
            }

            await _seatService.EnsureSeatsAsync(flightId, 30);

            ViewBag.FlightId = flightId;
            ViewBag.Passengers = passengers;
            ViewBag.JourneyClass = journeyClass;
            ViewBag.BasePrice = flight.BasePrice;
            ViewBag.FlightInfo = $"{flight.Airline.AirlineName} ({flight.FlightNumber})";

            return View(flight);
        }
    }
}