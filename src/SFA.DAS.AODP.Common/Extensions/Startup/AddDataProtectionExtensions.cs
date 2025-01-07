using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AODP.Models.Configuration;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddDataProtectionExtensions
{
    public static IServiceCollection AddDataProtectionKeys(this IServiceCollection services, string applicationName, IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services
                .AddDataProtection()
                .SetApplicationName(applicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keys")));

        }
        else
        {
            var redisConfiguration = configuration.GetRequiredSection("RedisConnectionSettings")
                .Get<RedisConnectionSettings>()!;

            var redisConnectionString = redisConfiguration.RedisConnectionString;
            var dataProtectionKeysDatabase = redisConfiguration.DataProtectionKeysDatabase;

            var redis = ConnectionMultiplexer
                .Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");


            services
                .AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, $"{applicationName}-DataProtectionKeys")
                .SetApplicationName(applicationName);
        }
        return services;
    }
}
