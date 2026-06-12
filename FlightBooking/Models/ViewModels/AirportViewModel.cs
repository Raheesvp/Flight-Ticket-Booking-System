

using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models.ViewModels
{
    public class AirportViewModel
    {
        public int AirportId { get; set; }

        [Required(ErrorMessage = "IATA code is required")]
        [StringLength(3, MinimumLength = 3,
            ErrorMessage = "IATA code must be exactly 3 letters")]
        [Display(Name = "IATA code")]
        public string IATACode { get; set; }

        [Required(ErrorMessage = "Airport name is required")]
        [StringLength(100)]
        [Display(Name = "Airport name")]
        public string AirportName { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50)]
        public string Country { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
