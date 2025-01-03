using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.ADPO.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class LoadConfigurationExtensions
    {
        public static IConfigurationRoot LoadConfiguration(this IConfiguration config, IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(config)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            // TODO: add external config from Azure Table Storage

            var configuration = configBuilder.Build();

            return configuration;
        }
    }
}
