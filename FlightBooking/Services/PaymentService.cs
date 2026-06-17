using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace FlightBooking.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;

        public PaymentService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string CreateOrder(decimal amount, string receiptId)
        {
            var client = new RazorpayClient(_config["Razorpay:KeyId"], _config["Razorpay:KeySecret"]);

            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", Convert.ToInt32(amount * 100) }, // Razorpay processes amounts in paisa/sub-units
                { "currency", "INR" },
                { "receipt", receiptId }
            };

            Order order = client.Order.Create(options);
            return order["id"].ToString();
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            try
            {
                string secret = _config["Razorpay:KeySecret"] ?? string.Empty;
                string payload = $"{orderId}|{paymentId}";

                var keyByte = Encoding.UTF8.GetBytes(secret);
                using var hmacsha256 = new HMACSHA256(keyByte);
                var messageBytes = Encoding.UTF8.GetBytes(payload);
                var hash = hmacsha256.ComputeHash(messageBytes);

                string generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return generatedSignature == signature;
            }
            catch
            {
                return false;
            }
        }
    }
    
}
