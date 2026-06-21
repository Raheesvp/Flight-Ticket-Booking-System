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

        // Metric KPI Cards Counters
     
        public int TotalBookingsCount { get; set; }
        public int ActiveFlightsCount { get; set; }
        public int TotalRegisteredPassengers { get; set; }

        // Chart Data Projections (Serialized to arrays in the View)
        public List<string> RevenueLabels { get; set; } = new List<string>();
        public List<decimal> RevenueDataPoints { get; set; } = new List<decimal>();

        public List<string> AirlineLabels { get; set; } = new List<string>();
        public List<int> AirlineBookingCounts { get; set; } = new List<int>();
    }
}
