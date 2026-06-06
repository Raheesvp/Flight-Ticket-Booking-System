
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Tables — you will add DbSets from Day 2 onward
        // public DbSet<Flight> Flights { get; set; }
        // public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Fluent API configurations go here (Day 3)
        }
    }
}