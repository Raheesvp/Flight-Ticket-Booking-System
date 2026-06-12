

using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8,
           ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword",
            ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm new password")]
        
        public string ConfirmPassword { get; set; }
    }
}