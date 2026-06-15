using FlightBooking.Models.ViewModels;
using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Controllers
{
    public class FlightController : Controller
    {

        private readonly AppDbContext _dbcontext;
       
        public FlightController(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

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

            return View(results);
        
    }
    }
}
