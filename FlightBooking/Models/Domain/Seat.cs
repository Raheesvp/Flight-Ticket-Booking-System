

namespace FlightBooking.Models.Domain
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int FlightId { get; set; }

        public string SeatNumber { get; set; }     // 1A, 1B, 12C
        public string SeatClass { get; set; }      // Economy | Business
        public bool IsAvailable { get; set; } = true;

        public Flight Flight { get; set; }
    }
}
