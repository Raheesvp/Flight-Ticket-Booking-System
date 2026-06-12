


using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models.ViewModels
{
    public class AirlineViewModel
    {
        public int AirlineId { get; set; }

        [Required(ErrorMessage = "Airline code is required")]
        [StringLength(3)]
        [Display(Name = "Airline code")]
        public string AirlineCode { get; set; }

        [Required(ErrorMessage = "Airline name is required")]
        [StringLength(100)]
        [Display(Name = "Airline name")]
        public string AirlineName { get; set; }

        [Display(Name = "Logo URL")]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
