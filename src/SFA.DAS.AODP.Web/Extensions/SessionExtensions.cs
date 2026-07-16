using System.Text.Json;

namespace SFA.DAS.AODP.Web.Extensions;

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        session.SetString(key, json);
    }

    public static T? GetObject<T>(this ISession session, string key) where T : class
    {
        var json = session.GetString(key);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<T>(json);
    }
}
