
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Web.Models.ViewModels
{
    public class FlightViewModel
    {
         public int FlightId { get; set; }

        [Required(ErrorMessage = "Flight number required")]
        [StringLength(10)]
        [Display(Name = "Flight number")]
        public string FlightNumber { get; set; }

        [Required(ErrorMessage = "Airline required")]
        [Display(Name = "Airline")]
        public int AirlineId { get; set; }

        [Required(ErrorMessage = "From airport required")]
        [Display(Name = "From")]
        public int FromAirportId { get; set; }

        [Required(ErrorMessage = "To airport required")]
        [Display(Name = "To")]
        public int ToAirportId { get; set; }

        [Required(ErrorMessage = "Departure time required")]
        [Display(Name = "Departure")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival time required")]
        [Display(Name = "Arrival")]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Range(1, 600,
            ErrorMessage = "Duration must be 1–600 mins")]
        [Display(Name = "Duration (mins)")]
        public int DurationMinutes { get; set; }

        [StringLength(50)]
        [Display(Name = "Aircraft type")]
        public string? AircraftType { get; set; }

        [Required]
        [Range(1, 900,
            ErrorMessage = "Total seats must be 1–900")]
        [Display(Name = "Total seats")]
        public int TotalSeats { get; set; }

        [Required]
        [Range(0, 9999999,
            ErrorMessage = "Base price must be positive")]
        [Display(Name = "Base price (₹)")]
        public decimal BasePrice { get; set; }

        public string Status { get; set; } = "Scheduled";

        // Dropdowns — populated in controller
        public IEnumerable<SelectListItem>? Airlines  { get; set; }
        public IEnumerable<SelectListItem>? Airports  { get; set; }
    }
}