using FlightBooking.Data.Repositories;
using FlightBooking.Models.Domain;
using FlightBooking.Web.Data;

namespace FlightBooking.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<Airport> Airports { get; private set; }
        public IRepository<Airline> Airlines { get; private set; }
        public IRepository<Flight> Flights { get; private set; }
        public IRepository<Booking> Bookings { get; private set; }
        public IRepository<Passenger> Passengers { get; private set; }
        public IRepository<Seat> Seats { get; private set; }
        public IRepository<Payment> Payments { get; private set; }

        
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Airports = new GenericRepository<Airport>(context);
            Airlines = new GenericRepository<Airline>(context);
            Flights = new GenericRepository<Flight>(context);
            Bookings = new GenericRepository<Booking>(context);
            Passengers = new GenericRepository<Passenger>(context);
            Seats = new GenericRepository<Seat>(context);
            Payments = new GenericRepository<Payment>(context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();

    }
}
