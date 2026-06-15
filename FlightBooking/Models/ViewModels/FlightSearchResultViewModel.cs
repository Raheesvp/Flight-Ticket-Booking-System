namespace FlightBooking.Models.ViewModels
{
    public class FlightSearchResultViewModel
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public string AirlineName { get; set; }
        public string? AirlineLogoUrl { get; set; }
        public string FromIATA { get; set; }
        public string FromCity { get; set; }
        public string ToIATA { get; set; }
        public string ToCity { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        public int AvailableSeats { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string AircraftType { get; set; }

        // Helpers for view
        public string DurationDisplay =>
            $"{DurationMinutes / 60}h {DurationMinutes % 60}m";

        public bool HasSeats =>
            AvailableSeats > 0;
    }
}
