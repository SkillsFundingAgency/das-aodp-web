using Microsoft.Extensions.Caching.Memory;

namespace SFA.DAS.AODP.Infrastructure.MemoryCache;

public class CacheManager : ICacheManager
{
    private readonly IMemoryCache _memoryCache;

    public CacheManager(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // Use IMemoryCache to get and set cache items
    public T Get<T>(string key)
    {
        if (!_memoryCache.TryGetValue(key, out T value))
        {
            return default; // Return default value if not found in cache
        }
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSize(1) // Set a size for this cache entry (optional)
            .SetSlidingExpiration(slidingExpiration ?? TimeSpan.FromMinutes(30)) // Default sliding expiration
            .SetAbsoluteExpiration(absoluteExpiration ?? TimeSpan.FromHours(1)); // Default absolute expiration

        _memoryCache.Set(key, value, cacheOptions);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }
}