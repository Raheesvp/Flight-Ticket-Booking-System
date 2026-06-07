namespace FlightBooking.Models.Domain
{
    public class Flight
    {
        public int FlightId { get; set; }

        public string FlightNumber { get; set; }   // e.g. 6E-201

        public int AirlineId { get; set; }         // FK
        public int FromAirportId { get; set; }     // FK
        public int ToAirportId { get; set; }       // FK

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }

        public string AircraftType { get; set; }  // Boeing 737
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal BasePrice { get; set; }
        public string Status { get; set; } = "Scheduled";

        public Airline Airline { get; set; }

        public Airport FromAirport { get; set; }
        public Airport ToAirport { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Seat> Seats { get; set; }
    }
}
