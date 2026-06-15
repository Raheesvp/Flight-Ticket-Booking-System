using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;


namespace FlightBooking.Models.ViewModels
{
    public class SearchViewModel
    {
        [Required]
        public int AirportId { get; set; }

        public int FromAirportId { get; set; }

        public int ToAirportId {get;set;}

        public DateTime DepartureDate { get; set; } = DateTime.Today;

        [Range(1, 9)]
        public int Passengers { get; set; } = 1;

        public string JourneyClass { get; set; } = "Economy";

        public string TripType { get; set; } = "0";


    }
}
