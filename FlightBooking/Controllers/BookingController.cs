using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FlightBooking.Web.Models.ViewModels;

namespace FlightBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;

    public BookingController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
    public async Task<IActionResult> Start(int flightId,int passengers,string seatIds)
        {
            if (flightId <= 0 || passengers <= 0 || string.IsNullOrEmpty(seatIds))
            {
                return RedirectToAction("Index", "Home");
            }

            var flight = await _db.Flights
                .Include(f => f.Airline)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);

            if (flight == null) return NotFound();

            // Parse selected seat identifiers split out from the query string
            var seatIdList = seatIds.Split(',').Select(int.Parse).ToList();

            if (seatIdList.Count != passengers)
            {
                TempData["Error"] = "Selected seats count must match total passenger count.";
                return RedirectToAction("SeatMap", "Flight", new { flightId, passengers });
            }

            // Retrieve matching seat records to verify cabin classes and compute the price matrix
            var seats = await _db.Seats
                .Where(s => seatIdList.Contains(s.SeatId) && s.FlightId == flightId)
                .ToListAsync();

            if (seats.Count != seatIdList.Count || seats.Any(s => !s.IsAvailable))
            {
                TempData["Error"] = "Some selected seats are no longer available.";
                return RedirectToAction("SeatMap", "Flight", new { flightId, passengers });
            }

            decimal computedTotal = 0;
            var viewModel = new PassengerDetailsVM
            {
                FlightId = flightId,
                PassengerCount = passengers,
                SeatIdsString = seatIds
            };

            for (int i = 0; i < passengers; i++)
            {
                var targetSeat = seats[i];
                decimal premiumMultiplier = targetSeat.SeatClass == "Business" ? 1.5m : 1.0m;
                computedTotal += (flight.BasePrice * premiumMultiplier);

                viewModel.Passengers.Add(new PassengerInputModel
                {
                    AssignedSeatId = targetSeat.SeatId,
                    AssignedSeatNumber = targetSeat.SeatNumber
                });
            }

            // Sync structural tokens securely to Session storage
            var sessionData = new BookingSessionVM
            {
                FlightId = flightId,
                PassengerCount = passengers,
                SelectedSeatIds = seatIdList,
                TotalAmount = computedTotal
            };
            HttpContext.Session.SetString("CurrentBookingSession", JsonSerializer.Serialize(sessionData));

            ViewBag.FlightDetails = $"{flight.Airline.AirlineName} - {flight.FlightNumber}";
            ViewBag.RouteDetails = $"{flight.FromAirport.City} ({flight.FromAirport.IATA}) to {flight.ToAirport.City} ({flight.ToAirport.IATA})";
            ViewBag.TotalAmount = computedTotal;

            return View("PassengerDetails", viewModel);
        }

        [HttpPost]

        [ValidateAntiForgeryToken]
        public IActionResult SavePassengers(PassengerDetailsVM model)
        {
            // Business Rule Validation before standard checking
            if (model.Passengers == null || model.Passengers.Count != model.PassengerCount)
            {
                ModelState.AddModelError("", "Passenger list configuration is corrupted.");
            }

            if (!ModelState.IsValid)
            {
                return View("PassengerDetails", model);
            }

            // Serialize captured form details into Session storage for the next summary steps
            HttpContext.Session.SetString("CapturedPassengerManifest", JsonSerializer.Serialize(model.Passengers));

            // Post-Redirect-Get pattern compliance
            TempData["Success"] = "Passenger details captured successfully!";
            return RedirectToAction("Summary");
        }

        [HttpGet]
        public IActionResult Summary()
        {
            // Placeholder endpoint to fulfill PR validation paths cleanly for Day 13
            var sessionStr = HttpContext.Session.GetString("CurrentBookingSession");
            if (string.IsNullOrEmpty(sessionStr)) return RedirectToAction("Index", "Home");

            ViewBag.SuccessMessage = TempData["Success"];
            return System.Web.Mvc.HttpVerbs.Get == 0 ? Content("Booking Funnel State Locked. Ready for Day 14 Checkout.") : Content("Booking Funnel State Locked. Ready for Day 14 Checkout.");
        }


    }

}

