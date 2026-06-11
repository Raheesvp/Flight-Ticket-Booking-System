using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Models.ViewModels
{
    public class AdminDashboardViewModel
    {

        // KPI cards
        public int TotalBookings { get; set; }
        public int BookingsToday { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalFlights { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAirports { get; set; }

        // Booking status breakdown
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int PendingBookings { get; set; }
    }
}
