namespace SFA.DAS.AODP.Application.MemoryCache
{
    public interface ICacheManager
    {
        T Get<T>(string key);
        void Remove(string key);
        void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    }
}