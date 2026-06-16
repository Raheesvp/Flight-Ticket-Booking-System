using FlightBooking.Services;
using FlightBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Controllers
{
    [Route("api/seats")]
    [ApiController]
    public class SeatController : Controller
    {
        private readonly SeatService _seatService;
        private readonly AppDbContext _dbContext;

        public SeatController(SeatService seatService, AppDbContext dbContext)
        {
            _seatService = seatService;
            _dbContext = dbContext;
        }

        [HttpGet("{flightId}")]

        public async Task<IActionResult> GetSeats(int flightId)
        {
            await _seatService.EnsureSeatsAsync(flightId,30);

            var seats = await _dbContext.Seats.Where(s => s.FlightId == flightId).Select(s =>
                        new
                        {
                            s.SeatId,
                            s.SeatNumber,
                            s.SeatClass,
                            s.IsAvailable
                        }
            ).ToListAsync();

            return Ok(seats);
        }
    }

}
