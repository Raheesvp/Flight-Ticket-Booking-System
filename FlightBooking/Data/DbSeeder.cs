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
                context.SaveChanges();
            }

            if (!context.Airlines.Any())
            {
                context.Airlines.AddRange(
                    new Airline { AirlineCode = "6E", AirlineName = "IndiGo", LogoUrl = "/images/airlines/indigo.png" },
                    new Airline { AirlineCode = "AI", AirlineName = "Air India", LogoUrl = "/images/airlines/airindia.png" },
                    new Airline { AirlineCode = "SG", AirlineName = "SpiceJet", LogoUrl = "/images/airlines/spicejet.png" },
                    new Airline { AirlineCode = "EK", AirlineName = "Emirates", LogoUrl = "/images/airlines/emirates.png" }
                );
                context.SaveChanges();
            }

            if (context.Flights.Count() < 50)
            {
                var existing = context.Flights.ToList();
                context.Flights.RemoveRange(existing);
                context.SaveChanges();

                var cok = context.Airports.First(a => a.IATACode == "COK");
                var del = context.Airports.First(a => a.IATACode == "DEL");
                var bom = context.Airports.First(a => a.IATACode == "BOM");
                var dxb = context.Airports.First(a => a.IATACode == "DXB");
                var maa = context.Airports.First(a => a.IATACode == "MAA");
                var hyd = context.Airports.First(a => a.IATACode == "HYD");

                var indigo = context.Airlines.First(a => a.AirlineCode == "6E");
                var airindia = context.Airlines.First(a => a.AirlineCode == "AI");
                var emirates = context.Airlines.First(a => a.AirlineCode == "EK");
                var spicejet = context.Airlines.First(a => a.AirlineCode == "SG");

                var routes = new[]
                {
                    (From: cok, To: del, Airline: indigo, FlightNum: "6E-201", BasePrice: 5500m, Time: 10, Duration: 180, Aircraft: "Airbus A320"),
                    (From: del, To: cok, Airline: indigo, FlightNum: "6E-202", BasePrice: 5600m, Time: 14, Duration: 180, Aircraft: "Airbus A320"),
                    
                    (From: del, To: bom, Airline: airindia, FlightNum: "AI-101", BasePrice: 6000m, Time: 11, Duration: 120, Aircraft: "Boeing 787"),
                    (From: bom, To: del, Airline: airindia, FlightNum: "AI-102", BasePrice: 6100m, Time: 15, Duration: 120, Aircraft: "Boeing 787"),
                    
                    (From: bom, To: dxb, Airline: emirates, FlightNum: "EK-502", BasePrice: 15000m, Time: 18, Duration: 200, Aircraft: "Boeing 777"),
                    (From: dxb, To: bom, Airline: emirates, FlightNum: "EK-503", BasePrice: 16000m, Time: 22, Duration: 200, Aircraft: "Boeing 777"),
                    
                    (From: cok, To: bom, Airline: spicejet, FlightNum: "SG-301", BasePrice: 4800m, Time: 8, Duration: 100, Aircraft: "Boeing 737"),
                    (From: bom, To: cok, Airline: spicejet, FlightNum: "SG-302", BasePrice: 4900m, Time: 12, Duration: 100, Aircraft: "Boeing 737"),
                    
                    (From: del, To: maa, Airline: indigo, FlightNum: "6E-401", BasePrice: 6500m, Time: 9, Duration: 160, Aircraft: "Airbus A321"),
                    (From: maa, To: del, Airline: indigo, FlightNum: "6E-402", BasePrice: 6400m, Time: 13, Duration: 160, Aircraft: "Airbus A321"),
                    
                    (From: hyd, To: del, Airline: airindia, FlightNum: "AI-501", BasePrice: 5200m, Time: 7, Duration: 130, Aircraft: "Airbus A319"),
                    (From: del, To: hyd, Airline: airindia, FlightNum: "AI-502", BasePrice: 5300m, Time: 16, Duration: 130, Aircraft: "Airbus A319"),
                    
                    (From: cok, To: dxb, Airline: airindia, FlightNum: "AI-933", BasePrice: 12000m, Time: 20, Duration: 240, Aircraft: "Boeing 787"),
                    (From: dxb, To: cok, Airline: airindia, FlightNum: "AI-934", BasePrice: 13000m, Time: 1, Duration: 240, Aircraft: "Boeing 787")
                };

                for (int i = 0; i <= 30; i++)
                {
                    var date = DateTime.Today.AddDays(i);
                    foreach (var r in routes)
                    {
                        context.Flights.Add(new Flight
                        {
                            FlightNumber = $"{r.FlightNum}-{i}",
                            AirlineId = r.Airline.AirlineId,
                            FromAirportId = r.From.AirportId,
                            ToAirportId = r.To.AirportId,
                            DepartureTime = date.AddHours(r.Time),
                            ArrivalTime = date.AddHours(r.Time).AddMinutes(r.Duration),
                            DurationMinutes = r.Duration,
                            AircraftType = r.Aircraft,
                            TotalSeats = 180,
                            AvailableSeats = 180,
                            BasePrice = r.BasePrice,
                            Status = "Scheduled"
                        });
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
