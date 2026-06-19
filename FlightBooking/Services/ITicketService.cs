using FlightBooking.Models.Domain;

namespace FlightBooking.Services
{
    public interface ITicketService
    {
        Task<byte[]> GenerateTicketPdfAsync(Booking booking);
    }
}
