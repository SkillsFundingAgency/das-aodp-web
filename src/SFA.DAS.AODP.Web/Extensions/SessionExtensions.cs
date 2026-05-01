using System.Text.Json;

namespace SFA.DAS.AODP.Web.Extensions;

public static class SessionExtensions
{
    extension(ISession session)
    {
        public void SetObject<T>(string key, T value) where T : class
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public T? GetObject<T>(string key) where T : class
        {
            var json = session.GetString(key);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<T>(json);
        }
    }
}
