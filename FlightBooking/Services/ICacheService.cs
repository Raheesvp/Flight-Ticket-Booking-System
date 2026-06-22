using System;
using System.Threading.Tasks;

namespace FlightBooking.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string cacheKey);
        Task SetAsync<T>(string cacheKey, T value, TimeSpan? absoluteExpiration = null);

        Task RemoveAsync(string cacheKey);
    }
}
