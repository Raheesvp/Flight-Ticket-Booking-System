namespace FlightBooking.Models.Domain
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int BookingId { get; set; }          // FK
        public string Title { get; set; }           // Mr / Mrs / Ms
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassengerType { get; set; }  // Adult|Child|Infant
        public string PassportNumber { get; set; } // international only
        public string SeatNumber { get; set; }     // e.g. 12A

        public Booking booking { get; set; }

    }
}
