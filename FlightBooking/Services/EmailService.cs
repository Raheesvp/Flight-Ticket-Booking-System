using EllipticCurve.Utils;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.ComponentModel;

namespace FlightBooking.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;


        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, byte[] attachmentBytes, string fileName)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("SendGrid API configuration token key is missing from appsettings properties.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);

            // Encode binary ticket payload data into clean Base64 strings for standard MIME transport
            if (attachmentBytes != null && attachmentBytes.Length > 0)
            {
                string base64Content = Convert.ToBase64String(attachmentBytes);
                msg.AddAttachment(fileName, base64Content, "application/pdf", "attachment");
            }

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                // Capture failures internally within infrastructure metrics logs
                string errorLog = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid communication failure token encountered: {response.StatusCode}. Details: {errorLog}");
            }
        }
    }
}
