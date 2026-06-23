using System.Text.Encodings.Web;

namespace FlightBooking.Services
{
    public class SanitizerService : ISanitizerService
    {
        private readonly HtmlEncoder _htmlEncoder;

        public SanitizerService(HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
        }

        public string SanitizeInput(string dirtyInput)
        {
            if (string.IsNullOrWhiteSpace(dirtyInput))
            {
                return dirtyInput;
            }

            string trimmed = dirtyInput.Trim();
            return _htmlEncoder.Encode(trimmed);
        }
    }
}
