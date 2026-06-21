

namespace FlightBooking.Web.Models.ViewModels
{
    public class CancellationResultViewModel
    {
        public string PNR { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CancellationPenalty { get; set; }
        public string RefundReferenceId { get; set; }
        public string WebhookSimulationStatus { get; set; }
    }
}