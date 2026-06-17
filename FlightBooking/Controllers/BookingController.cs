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
            ViewBag.RouteDetails = $"{flight.FromAirport.City} ({flight.FromAirport.IATACode}) to {flight.ToAirport.City} ({flight.ToAirport.IATACode})";
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
        public async Task<IActionResult> Summary()
        {
            var sessionStr = HttpContext.Session.GetString("CurrentBookingSession");
            var manifestStr = HttpContext.Session.GetString("CapturedPassengerManifest");

            if (string.IsNullOrEmpty(sessionStr) || string.IsNullOrEmpty(manifestStr))
            {
                TempData["Error"] = "Your booking session has expired. Please select your flight again.";
                return RedirectToAction("Index", "Home");
            }

            var sessionData = JsonSerializer.Deserialize<BookingSessionVM>(sessionStr);
            var passengersList = JsonSerializer.Deserialize<List<PassengerInputModel>>(manifestStr);

            if (sessionData == null || passengersList == null) return RedirectToAction("Index", "Home");

            var flight = await _db.Flights
                .Include(f => f.Airline)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(f => f.FlightId == sessionData.FlightId);

            if (flight == null) return NotFound();

            // Mocking structural catalogs (to be mapped to database configuration rows on Day 15)
            var baggageOptions = new List<BaggageOptionDTO>
            {
                new BaggageOptionDTO { Id = 1, Label = "Standard Cabin (7 KG Included)", Price = 0.00m },
                new BaggageOptionDTO { Id = 2, Label = "Medium Check-In (15 KG)", Price = 450.00m },
                new BaggageOptionDTO { Id = 3, Label = "Heavy Check-In (25 KG)", Price = 950.00m }
            };

            var mealOptions = new List<MealOptionDTO>
            {
                new MealOptionDTO { Id = 1, Name = "No Meal Service Choice", Price = 0.00m },
                new MealOptionDTO { Id = 2, Name = "Vegetarian Hot Platter", Price = 250.00m },
                new MealOptionDTO { Id = 3, Name = "Continental Diabetic Meal", Price = 350.00m }
            };

            var viewModel = new BookingSummary
            {
                FlightId = flight.FlightId,
                Flight = flight,
                PassengerCount = sessionData.PassengerCount,
                BaseFareAndSeatsTotal = sessionData.TotalAmount,
                AvailableBaggage = baggageOptions,
                AvailableMeals = mealOptions
            };

            for (int i = 0; i < passengersList.Count; i++)
            {
                viewModel.Passengers.Add(new PassengerSummaryItemVM
                {
                    Index = i,
                    Name = passengersList[i].Name,
                    Age = passengersList[i].Age,
                    Gender = passengersList[i].Gender,
                    SeatId = passengersList[i].AssignedSeatId,
                    SeatNumber = passengersList[i].AssignedSeatNumber
                });

                // Initialize empty configuration array trackers for binding
                viewModel.Selections.Add(new PassengerAddOnSelection
                {
                    PassengerIndex = i,
                    SelectedBaggageId = 1, // default to index item 1
                    SelectedMealId = 1     // default to index item 1
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAddOns(BookingSummary model)
        {
            if (model.Selections == null || model.Selections.Count == 0)
            {
                ModelState.AddModelError("", "Add-on structure mapping configuration is invalid.");
                return RedirectToAction(nameof(Summary));
            }

            // Save auxiliary selections securely back into the user session state
            HttpContext.Session.SetString("FinalAncillarySelections", JsonSerializer.Serialize(model.Selections));

            TempData["Success"] = "Ancillary add-ons mapped successfully! Flight state is locked.";

            // Post-Redirect-Get pattern compliance targeting Day 15 Payment module Gateway
            return RedirectToAction("GatewayRedirect");
        }

        [HttpGet]
        public IActionResult GatewayRedirect()
        {
            return Content("Booking funnel state locked down. Ready for Day 15 Razorpay Payment integration.");
        }


    }

}

