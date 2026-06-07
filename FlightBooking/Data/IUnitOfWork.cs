using FlightBooking.Data.Repositories;
using FlightBooking.Models.Domain;

namespace FlightBooking.Data
{
    public interface IUnitOfWork : IDisposable
    {
        // One repo property per entity
        IRepository<Airport> Airports { get; }
        IRepository<Airline> Airlines { get; }
        IRepository<Flight> Flights { get; }
        IRepository<Booking> Bookings { get; }
        IRepository<Passenger> Passengers { get; }
        IRepository<Seat> Seats { get; }
        IRepository<Payment> Payments { get; }

        // Single save — commits all changes at once
        Task<int> SaveChangesAsync();
    }
}
