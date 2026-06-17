using System.ComponentModel.DataAnnotations.Schema;

namespace FlightBooking.Models.Domain
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }          // FK
        public string Title { get; set; } = "Mr";           // Mr / Mrs / Ms
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
        public string PassengerType { get; set; } = "Adult";  // Adult|Child|Infant
        public string PassportNumber { get; set; } = string.Empty; // international only
        public string SeatNumber { get; set; } = string.Empty;     // e.g. 12A

        public Booking booking { get; set; }

        [NotMapped]
        public string FullName
        {
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                var parts = value?.Split(new[] { ' ' }, 2) ?? new string[0];
                FirstName = parts.Length > 0 ? parts[0] : string.Empty;
                LastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
        }

        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
            set
            {
                DateOfBirth = DateTime.Today.AddYears(-value);
                PassengerType = value >= 12 ? "Adult" : "Child";
            }
        }

        [NotMapped]
        public string Gender
        {
            get
            {
                if (Title == "Mr") return "Male";
                if (Title == "Mrs" || Title == "Ms") return "Female";
                return "Other";
            }
            set
            {
                if (value == "Male") Title = "Mr";
                else if (value == "Female") Title = "Ms";
                else Title = "Other";
            }
        }
    }
}
