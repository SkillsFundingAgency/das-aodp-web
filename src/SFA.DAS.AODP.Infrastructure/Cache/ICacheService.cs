namespace SFA.DAS.AODP.Infrastructure.Cache
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<T> GetAsync<T>(string key, Func<Task<T>> getValueFunc) where T : class;
        Task SetAsync<T>(string key, T value) where T : class;
        Task RemoveAsync(string key);
    }
}