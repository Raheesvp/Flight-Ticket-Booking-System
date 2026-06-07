
using FlightBooking.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets — each becomes a SQL table
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Flight has TWO FKs to Airport — must configure manually
            // to avoid EF cascade delete conflict
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.FromAirport)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.FromAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ToAirport)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ToAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Airport IATA code
            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATACode)
                .IsUnique();

            // Unique constraint on PNR
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PNR)
                .IsUnique();
        }
    }
}