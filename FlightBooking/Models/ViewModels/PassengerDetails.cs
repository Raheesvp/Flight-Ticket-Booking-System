
using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Web.Models.ViewModels
{

    public class PassengerDetailsVM
    {
        public int FlightId { get; set; }
        public int PassengerCount { get; set; }

        public string SeatIdsString { get; set; } = string.Empty;

        public List<PassengerInputModel> Passengers { get; set; } = new List<PassengerInputModel>();

    }

    public class PassengerInputModel
    {
        [Required(ErrorMessage = "Passenger name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Prompt = "Enter Full Name as in Passport/ID")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Gender selection is required")]
        public string Gender { get; set; } = string.Empty;

        public int AssignedSeatId { get; set; }
        public string AssignedSeatNumber { get; set; } = string.Empty;
    }
}
