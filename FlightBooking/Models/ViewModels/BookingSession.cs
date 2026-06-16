namespace FlightBooking.Web.Models.ViewModels
{
    public class BookingSessionVM
    {
        public int FlightId { get; set; }

        public int PassengerCount { get; set; }

        public List<int> SelectedSeatIds { get; set; } = new List<int>();
        
        public decimal TotalAmount { get; set; }
    }
}