namespace FlightBooking.Models.Domain
{
    public class Airport
    {
        public int AirportId { get; set; }
        public string IATACode { get; set; }

        public string AirportName { get; set; }

        public string City { get; set; }

        public string Country { get; set; } 

        public bool IsActive { get; set; }

        //navigation
        
        public ICollection<Flight> DepartureFlights { get; set; }

        public ICollection<Flight> ArrivalFlights { get; set; }


    }
}
