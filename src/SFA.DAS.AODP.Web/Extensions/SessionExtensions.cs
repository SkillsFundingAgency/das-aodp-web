using Newtonsoft.Json;

namespace SFA.DAS.AODP.Web.Extensions;

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) where T : class
    {
        var json = JsonConvert.SerializeObject(value);
        session.SetString(key, json);
    }

    public static T? GetObject<T>(this ISession session, string key) where T : class
    {
        var json = session.GetString(key);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonConvert.DeserializeObject<T>(json);
    }
}
