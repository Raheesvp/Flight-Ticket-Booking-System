

namespace FlightBooking.Models.Domain
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }          // FK
        public string TransactionId { get; set; }  // from Razorpay
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }  // Card|UPI|NetBanking
        public string Status { get; set; }         // Success|Failed|Pending
        public DateTime PaidAt { get; set; }

        public Booking Booking { get; set; }
    }
}
