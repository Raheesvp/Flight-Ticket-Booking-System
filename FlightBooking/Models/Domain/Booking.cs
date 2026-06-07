namespace FlightBooking.Models.Domain
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string PNR { get; set; }            // e.g. AB1234
        public string UserId { get; set; }          // FK → AspNetUsers
        public int FlightId { get; set; }           // FK

        public DateTime BookingDate { get; set; }
        public string JourneyClass { get; set; }   // Economy | Business
        public int TotalPassengers { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Confirmed";
        // Confirmed | Cancelled | Pending


        public Flight Flight { get; set; }

        public ICollection<Passenger> Passengers { get; set; }
        public Payment Payment { get; set; }

    }
}
