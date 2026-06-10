using FlightBooking.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace FlightBooking.Web.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Step 1 — seed roles
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
            }

            // Step 2 — seed default admin user
            var adminEmail = "admin@flightbooking.com";

            if (await userManager
                    .FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    FullName           = "Site Admin",
                    EmailConfirmed = true,
                    IsActive       = true
                };

                var result = await userManager
                    .CreateAsync(admin, "Admin@1234");

                if (result.Succeeded)
                    await userManager
                        .AddToRoleAsync(admin, "Admin");
            }
        }
    }
}