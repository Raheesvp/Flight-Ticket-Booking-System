using Microsoft.AspNetCore.Identity;

namespace FlightBooking.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        //IdentityUser Already gives you 
        //Id, Email, UserName, PhoneNumber, PasswordHash

        public string Name { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

    }
}
