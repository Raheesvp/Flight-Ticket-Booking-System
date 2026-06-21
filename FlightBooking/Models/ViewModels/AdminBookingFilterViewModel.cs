
using System;

namespace FlightBooking.Web.Models.ViewModels
{
    public class AdminBookingFilterViewModel
    {
        public string SearchPnr { get; set; }
        public int? FilterAirlineId { get; set; }
        public DateTime? FilterDate { get; set; }

        // Pagination Elements
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}