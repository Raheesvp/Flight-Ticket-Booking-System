using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace FlightBooking.Services
{
    public class CacheService:ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            try
            {
                if (_cache.TryGetValue(cacheKey, out T cachedValue))
                {
                    return await Task.FromResult(cachedValue);
                }
                return default;
            }
            catch
            {
                return default;
            }
        }

        public async Task SetAsync<T>(string cacheKey,T value,TimeSpan? absoluteExpiration = null)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10)
                };

                _cache.Set(cacheKey, value, cacheOptions);
                await Task.CompletedTask;
            }
            catch
            {
                // Ignore cache write failures
            }
        }

        public async Task RemoveAsync(string cacheKey)
        {
            try
            {
                _cache.Remove(cacheKey);
                await Task.CompletedTask;
            }
            catch
            {
                // Ignore removal failures
            }
        }
    }
}
