using FlightBooking.Models.Domain;
using FlightBooking.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Services
{
    public class SeatService
    {
        private readonly AppDbContext _dbcontext;

        public SeatService(AppDbContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task EnsureSeatsAsync(int FlightId, int TotalRows)
        {
            var seatsExist = await _dbcontext.Seats.AnyAsync(s => s.FlightId == FlightId);
            if (seatsExist) return;

            var seatsToCreate = new List<Seat>();
            string[] columns = { "A", "B", "C", "D", "E", "F" };

            for(int row = 1; row <= TotalRows; row++)
            {
                string seatClass = (row <= 4) ? "Business" : "Economy";

                foreach(var col in columns)
                {
                    seatsToCreate.Add(new Seat
                    {
                        FlightId = FlightId,
                        SeatNumber = $"{row}{col}",
                        SeatClass = seatClass,
                        IsAvailable = true
                    });
                }
            }

            await _dbcontext.Seats.AddRangeAsync(seatsToCreate);
            await _dbcontext.SaveChangesAsync();

        }
    }
}
