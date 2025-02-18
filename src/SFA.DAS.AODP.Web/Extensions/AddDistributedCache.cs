using SFA.DAS.AODP.Models.Settings;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddDistributedCacheExtension
{
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var redisConfiguration = configuration.GetRequiredSection("RedisConnectionSettings")
               .Get<RedisConnectionSettings>()!;
                options.Configuration = redisConfiguration.RedisConnectionString;
            });
        }
        return services;
    }
}
