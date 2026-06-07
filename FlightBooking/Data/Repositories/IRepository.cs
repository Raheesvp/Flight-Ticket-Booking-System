using System.Linq.Expressions;

namespace FlightBooking.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        //Read

        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate);

        //Write

        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);

        //Helpers

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();


    }
}
