namespace FlightBooking.Services
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, byte[] attachmentBytes, string fileName);
    }
}
