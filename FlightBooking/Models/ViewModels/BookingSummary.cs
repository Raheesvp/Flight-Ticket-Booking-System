
using FlightBooking.Models.Domain;

namespace FlightBooking.Web.Models.ViewModels
{
    public class BookingSummary
    {
        public int FlightId { get; set; }

        public Flight Flight { get; set; } = null!;

        public int PassengerCount { get; set; }

        public decimal BaseFareAndSeatsTotal { get; set; }

        public List<PassengerSummaryItemVM> Passengers { get; set; } = new List<PassengerSummaryItemVM>();

        public List<BaggageOptionDTO> AvailableBaggage { get; set; } = new List<BaggageOptionDTO>();
        public List<MealOptionDTO> AvailableMeals { get; set; } = new List<MealOptionDTO>();

        // Postback structural collections binding selections back to indices
        public List<PassengerAddOnSelection> Selections { get; set; } = new List<PassengerAddOnSelection>();
    }

    public class PassengerSummaryItemVM
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
    }

    public class BaggageOptionDTO
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty; // e.g., "Medium (15 KG)"
        public decimal Price { get; set; }
    }

    public class MealOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Vegetarian Hindu Meal"
        public decimal Price { get; set; }
    }

    public class PassengerAddOnSelection
    {
        public int PassengerIndex { get; set; }
        public int SelectedBaggageId { get; set; }
        public int SelectedMealId { get; set; }
    }
}
