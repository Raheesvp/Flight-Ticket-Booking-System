using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FlightBooking.Web.Models.ViewModels;
using FlightBooking.Data;
using FlightBooking.Services;
using FlightBooking.Models.ViewModels;
using FlightBooking.Models.Domain;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace FlightBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly PaymentService _paymentService;
        private readonly IConfiguration _config;
        private readonly ITicketService _ticketService;
        private readonly IEmailService _emailService;


    public BookingController(AppDbContext db, IUnitOfWork uow, PaymentService paymentService, IConfiguration config,ITicketService ticketService,IEmailService emailService)
        {
            _db = db;
            _uow = uow;
            _paymentService = paymentService;
            _config = config;
            _ticketService = ticketService;
            _emailService = emailService;
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
        public async Task<IActionResult> SavePassengers(PassengerDetailsVM model)
        {
            // Business Rule Validation before standard checking
            if (model.Passengers == null || model.Passengers.Count != model.PassengerCount)
            {
                ModelState.AddModelError("", "Passenger list configuration is corrupted.");
            }

            if (!ModelState.IsValid)
            {
                var flight = await _db.Flights
                    .Include(f => f.Airline)
                    .Include(f => f.FromAirport)
                    .Include(f => f.ToAirport)
                    .FirstOrDefaultAsync(f => f.FlightId == model.FlightId);

                if (flight != null)
                {
                    ViewBag.FlightDetails = $"{flight.Airline.AirlineName} - {flight.FlightNumber}";
                    ViewBag.RouteDetails = $"{flight.FromAirport.City} ({flight.FromAirport.IATACode}) to {flight.ToAirport.City} ({flight.ToAirport.IATACode})";
                }

                var sessionStr = HttpContext.Session.GetString("CurrentBookingSession");
                if (!string.IsNullOrEmpty(sessionStr))
                {
                    var sessionData = JsonSerializer.Deserialize<BookingSessionVM>(sessionStr);
                    ViewBag.TotalAmount = sessionData?.TotalAmount ?? 0m;
                }
                else
                {
                    ViewBag.TotalAmount = 0m;
                }

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
        public async Task<IActionResult> GatewayRedirect()
        {
            var sessionStr = HttpContext.Session.GetString("CurrentBookingSession");
            var ancillaryStr = HttpContext.Session.GetString("FinalAncillarySelections");

            if (string.IsNullOrEmpty(sessionStr)) return RedirectToAction("Index", "Home");

            var sessionData = JsonSerializer.Deserialize<BookingSessionVM>(sessionStr)!;
            var ancillarySelections = string.IsNullOrEmpty(ancillaryStr)
                ? new List<PassengerAddOnSelection>()
                : JsonSerializer.Deserialize<List<PassengerAddOnSelection>>(ancillaryStr)!;

            // Calculate total including add-ons
            decimal ancillaryTotal = 0;
            // (Mock price aggregation matching Day 14 UI matrices)
            foreach (var selection in ancillarySelections)
            {
                if (selection.SelectedBaggageId == 2) ancillaryTotal += 450.00m;
                if (selection.SelectedBaggageId == 3) ancillaryTotal += 950.00m;
                if (selection.SelectedMealId == 2) ancillaryTotal += 250.00m;
                if (selection.SelectedMealId == 3) ancillaryTotal += 350.00m;
            }

            decimal totalFinalAmount = sessionData.TotalAmount + ancillaryTotal;
            string transactionReceiptId = $"REC_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Call external SDK to register intent order
            string razorpayOrderId = _paymentService.CreateOrder(totalFinalAmount, transactionReceiptId);

            ViewBag.RazorpayKeyId = _config["Razorpay:KeyId"];
            ViewBag.OrderId = razorpayOrderId;
            ViewBag.AmountPaisa = Convert.ToInt32(totalFinalAmount * 100);
            ViewBag.FlightId = sessionData.FlightId;
            ViewBag.TotalAmount = totalFinalAmount;

            return View("GatewayCheckout");
        }
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] RazorpayPaymentModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.RazorpayPaymentId))
            {
                return Json(new { success = false, message = "Payment metadata transmission corrupted." });
            }

            bool isValid = _paymentService.VerifySignature(model.RazorpayOrderId, model.RazorpayPaymentId, model.RazorpaySignature);
            if (!isValid)
            {
                return Json(new { success = false, message = "Cryptographic signature check failed. Security alert triggered." });
            }

            var sessionStr = HttpContext.Session.GetString("CurrentBookingSession");
            var manifestStr = HttpContext.Session.GetString("CapturedPassengerManifest");

            if (string.IsNullOrEmpty(sessionStr) || string.IsNullOrEmpty(manifestStr))
            {
                return Json(new { success = false, message = "Session data lost mid-transaction. Contact support." });
            }

            var sessionData = JsonSerializer.Deserialize<BookingSessionVM>(sessionStr)!;
            var passengerInputs = JsonSerializer.Deserialize<List<PassengerInputModel>>(manifestStr)!;

            // Generate Unique Alpha-Numeric PNR code string
            string generatedPnr = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            string activeUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(activeUserId))
            {
                return Json(new { success = false, message = "User session is unauthenticated. Please log in to complete your booking." });
            }

            // Find the journey class from the first seat in the booking
            string journeyClass = "Economy";
            if (passengerInputs.Any())
            {
                var firstSeatId = passengerInputs.First().AssignedSeatId;
                var seat = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == firstSeatId);
                if (seat != null)
                {
                    journeyClass = seat.SeatClass;
                }
            }

            // Begin Transaction Sequence via Unit of Work / DB Context bounds
            using var dbTransaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var bookingRecord = new Booking
                {
                    FlightId = sessionData.FlightId,
                    UserId = activeUserId,
                    PNR = generatedPnr,
                    BookingDate = DateTime.UtcNow,
                    JourneyClass = journeyClass,
                    TotalPassengers = sessionData.PassengerCount,
                    TotalAmount = sessionData.TotalAmount, // updates inside production mapping triggers
                    Status = "Confirmed"
                };

                await _db.Bookings.AddAsync(bookingRecord);
                await _db.SaveChangesAsync();

                foreach (var input in passengerInputs)
                {
                    var passenger = new Passenger
                    {
                        BookingId = bookingRecord.BookingId,
                        FullName = input.Name,
                        Age = input.Age,
                        Gender = input.Gender,
                        SeatNumber = input.AssignedSeatNumber
                    };
                    await _db.Passengers.AddAsync(passenger);

                    // Update corresponding seat records to flag them as occupied
                    var seatEntity = await _db.Seats.FirstOrDefaultAsync(s => s.SeatId == input.AssignedSeatId);
                    if (seatEntity != null)
                    {
                        seatEntity.IsAvailable = false; // Seat locked out
                    }
                }

                var paymentRecord = new Payment
                {
                    BookingId = bookingRecord.BookingId,
                    TransactionId = model.RazorpayPaymentId,
                    Amount = sessionData.TotalAmount,
                    PaymentMethod = "UPI",
                    PaidAt = DateTime.UtcNow,
                    Status = "Success"
                };
                await _db.Payments.AddAsync(paymentRecord);

                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                // Clear temporary cache collections to finalize pipeline steps safely
                HttpContext.Session.Remove("CurrentBookingSession");
                HttpContext.Session.Remove("CapturedPassengerManifest");
                HttpContext.Session.Remove("FinalAncillarySelections");

                return Json(new { success = true, pnr = generatedPnr, bookingId = bookingRecord.BookingId });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Json(new { success = false, message = $"Internal transaction failure. Error details: {ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id, string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
            {
                return BadRequest("Invalid confirmation routing arguments.");
            }

            var booking = await _db.Bookings
                .Include(b => b.Passengers)
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport)
                .FirstOrDefaultAsync(b => b.PNR == pnr.Trim().ToUpper());

            if (booking == null)
            {
                return NotFound("Targeted reservation parameters could not be located.");
            }

            // Retrieve authenticated user's email marker address via claims matrix identity
            var userEmail = User.Identity?.Name ?? "customer@flightbooking.com";

            try
            {
                // 1. Generate the digital e-ticket PDF binary stream payload
                byte[] pdfBytes = await _ticketService.GenerateTicketPdfAsync(booking);
                string attachmentName = $"E-Ticket_{booking.PNR}.pdf";

                // 2. Build out localized inline-styled HTML receipt layout template
                string mailTemplate = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                        <div style='background-color: #1a1a2e; padding: 25px; text-align: center; color: #ffffff;'>
                            <h2 style='margin: 0; font-size: 24px; letter-spacing: 1px;'>YOUR FLIGHT IS CONFIRMED!</h2>
                            <p style='margin: 5px 0 0 0; color: #f05a22; font-weight: bold;'>PNR: {booking.PNR}</p>
                        </div>
                        <div style='padding: 30px; color: #333333; line-height: 1.6;'>
                            <h3 style='margin-top: 0; color: #1a1a2e;'>Dear Passenger,</h3>
                            <p>Thank you for choosing Alhind Flights. Your payment has cleared successfully, and your seating slots have been securely committed to our manifest records.</p>
                            
                            <hr style='border: 0; border-top: 1px solid #eeeeee; margin: 20px 0;' />
                            
                            <h4 style='color: #1a1a2e; margin-bottom: 5px;'>FLIGHT DETAILS</h4>
                            <p style='margin: 0;'><strong>Airline:</strong> {booking.Flight.Airline.AirlineName} ({booking.Flight.FlightNumber})</p>
                            <p style='margin: 0;'><strong>Route:</strong> {booking.Flight.FromAirport.City} ({booking.Flight.FromAirport.IATACode}) &rarr; {booking.Flight.ToAirport.City} ({booking.Flight.ToAirport.IATACode})</p>
                            <p style='margin: 0;'><strong>Departure Time:</strong> {booking.Flight.DepartureTime:dd MMM yyyy HH:mm}</p>
                            
                            <hr style='border: 0; border-top: 1px solid #eeeeee; margin: 20px 0;' />
                            
                            <p style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #f05a22; font-size: 13px; margin: 0;'>
                                <strong>Attachment Information:</strong> Your official digital e-ticket has been compiled and appended directly to this communication as an uncorrupted PDF attachment file. Please print or save this document for check-in procedures.
                            </p>
                        </div>
                        <div style='background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #777777; border-top: 1px solid #eeeeee;'>
                            &copy; {DateTime.UtcNow.Year} Alhind Flights Network. All rights reserved.
                        </div>
                    </div>";

                // 3. Dispatch the message asynchronously into the background mailing network
                await _emailService.SendEmailWithAttachmentAsync(
                    userEmail,
                    $"Flight Reservation Confirmation - PNR: {booking.PNR}",
                    mailTemplate,
                    pdfBytes,
                    attachmentName
                );
            }
            catch (Exception)
            {
                // ?? CRITICAL RESILIENCY: We catch the infrastructure error silently rather than crashing.
                // This ensures the customer is still presented with their web confirmation screen even if the mail provider fails.
                TempData["MailWarning"] = "Your booking was recorded successfully, but an issue occurred while mailing your ticket receipt wrapper. You can still download your e-ticket instantly below.";
            }

            return View(booking);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadTicket(string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
            {
                return BadRequest("Invalid or missing PNR configuration argument Token");
            }

            var booking = await _db.Bookings
        .Include(b => b.Passengers)
        .Include(b => b.Flight)
            .ThenInclude(f => f.Airline)
        .Include(b => b.Flight)
            .ThenInclude(f => f.FromAirport)
        .Include(b => b.Flight)
            .ThenInclude(f => f.ToAirport)
        .FirstOrDefaultAsync(b => b.PNR == pnr.Trim().ToUpper());

            if (booking == null)
            {
                return NotFound("The requested flight reservation context record was not found.");
            }

            // Optional Safety Check: Ensure the logged-in user matches the booking context owner
            var currentUserId = _db.Bookings.Where(b => b.PNR == pnr).Select(b => b.UserId).FirstOrDefault();
            if (booking.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            byte[] pdfBinaryPayload = await _ticketService.GenerateTicketPdfAsync(booking);

            // Construct streaming file wrapper parameters targeting instant client downloads
            string outputFileName = $"Ticket_{booking.PNR}.pdf";
            return File(pdfBinaryPayload, "application/pdf", outputFileName);
        }
    }
}


   

