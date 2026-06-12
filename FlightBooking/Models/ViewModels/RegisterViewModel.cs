

using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="FullName is Required")]
        [StringLength(50)]
        [Display(Name="Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage ="Email is Required")]
        [EmailAddress(ErrorMessage ="Enter a valid email")]

        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [Display(Name = "Phone number")]

        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8,
           ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password",
            ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
