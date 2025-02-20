using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SFA.DAS.AODP.Infrastructure.Cache
{
    public class CacheService : ICacheService
    {
        private TimeSpan CacheTimeSpan = TimeSpan.FromHours(1);

        private readonly IDistributedCache _distributedCache;

        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            string? cachedValue = await _distributedCache.GetStringAsync(key);
            if (cachedValue == null) return null;

            return JsonConvert.DeserializeObject<T>(cachedValue); ;
        }

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> getValueFunc) where T : class
        {
            T? cachedValue = await GetAsync<T>(key);

            if (cachedValue != null) return cachedValue;

            cachedValue = await getValueFunc();

            await SetAsync(key, cachedValue);

            return cachedValue;
        }

        public async Task SetAsync<T>(string key, T value) where T : class
        {
            string cacheValue = JsonConvert.SerializeObject(value);
            await _distributedCache.SetStringAsync(key, cacheValue, new DistributedCacheEntryOptions() { SlidingExpiration = CacheTimeSpan });
        }
    }
}
