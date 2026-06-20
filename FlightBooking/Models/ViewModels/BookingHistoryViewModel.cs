

using FlightBooking.Models.Domain;

namespace FlightBooking.Web.Models.ViewModels
{
    public class BookingHistoryViewModel
    {
        public List<Booking> ActiveBooking { get; set; } = new List<Booking>();
        public List<Booking> PastBooking { get; set; } = new List<Booking>();
    }
}