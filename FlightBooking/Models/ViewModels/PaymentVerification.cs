namespace FlightBooking.Models.ViewModels
{
    public class PaymentVerification
    {
        public string RazorpayOrderId { get; set; } = string.Empty;

        public string RazorpaymentId { get; set; } = string.Empty;

        public string RazorpaySignature { get; set; } = string.Empty;

        public int FlightId { get; set; }
    }
}
