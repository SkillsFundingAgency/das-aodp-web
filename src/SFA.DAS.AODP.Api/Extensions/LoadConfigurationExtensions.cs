using SFA.DAS.AODP.Common.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class LoadConfigurationExtensions
    {
        public static IConfigurationRoot LoadConfiguration(this IConfiguration config, IServiceCollection services, bool isDevelopment)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(config)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true);
#endif

            if (!isDevelopment)
            {
                configBuilder.LoadAzureTableStorage(config);
            }

            var configuration = configBuilder.Build();

            return configuration;
        }
    }
}
