
using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Web.Models.ViewModels
{

        public class ProfileViewModel
        {
            [Required(ErrorMessage = "Full name is required")]
            [StringLength(100)]
            [Display(Name = "Full name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            [Phone(ErrorMessage = "Enter a valid phone number")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            // For display only — not posted back
            public string? Email { get; set; }
            public string? CurrentPhotoUrl { get; set; }

            // Optional file upload
            [Display(Name = "Profile photo")]
            public IFormFile? ProfilePhoto { get; set; }
        }
}
