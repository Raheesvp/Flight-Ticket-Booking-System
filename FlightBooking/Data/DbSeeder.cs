using FlightBooking.Models.Domain;
using FlightBooking.Web.Data;
using Microsoft.Identity.Client;

namespace FlightBooking.Data
{
    public static  class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Only seed if tables are empty
            // Only seed if tables are empty
            if (!context.Airports.Any())
            {
                context.Airports.AddRange(
                    new Airport { IATACode = "COK", AirportName = "Cochin International", City = "Kochi", Country = "India" },
                    new Airport { IATACode = "DEL", AirportName = "Indira Gandhi International", City = "Delhi", Country = "India" },
                    new Airport { IATACode = "BOM", AirportName = "Chhatrapati Shivaji", City = "Mumbai", Country = "India" },
                    new Airport { IATACode = "MAA", AirportName = "Chennai International", City = "Chennai", Country = "India" },
                    new Airport { IATACode = "HYD", AirportName = "Rajiv Gandhi International", City = "Hyderabad", Country = "India" },
                    new Airport { IATACode = "DXB", AirportName = "Dubai International", City = "Dubai", Country = "UAE" }
                );
            }

            if (!context.Airlines.Any())
            {
                context.Airlines.AddRange(
                    new Airline { AirlineCode = "6E", AirlineName = "IndiGo", LogoUrl = "/images/airlines/indigo.png" },
                    new Airline { AirlineCode = "AI", AirlineName = "Air India", LogoUrl = "/images/airlines/airindia.png" },
                    new Airline { AirlineCode = "SG", AirlineName = "SpiceJet", LogoUrl = "/images/airlines/spicejet.png" },
                    new Airline { AirlineCode = "EK", AirlineName = "Emirates", LogoUrl = "/images/airlines/emirates.png" }
                );
            }

            context.SaveChanges();
        }
    }
}
