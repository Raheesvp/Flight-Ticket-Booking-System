

namespace FlightBooking.Models.Domain
{
    public class Airline
    {
        public int AirlineId { get; set; }  

        public string AirlineCode { get; set; }

        public string AirlineName { get; set; }

        public string LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Flight> Flights { get; set; }

    }
}
