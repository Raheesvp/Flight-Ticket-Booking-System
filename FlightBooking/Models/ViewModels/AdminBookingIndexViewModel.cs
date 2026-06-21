
using FlightBooking.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FlightBooking.Web.Models.ViewModels
{
    public class AdminBookingIndexViewModel
    {
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public AdminBookingFilterViewModel Filters { get; set; }

        // Metadata dropdown mappings for filter options
        public List<SelectListItem> AirlinesList { get; set; } = new List<SelectListItem>();

        // Pagination Metadata
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}