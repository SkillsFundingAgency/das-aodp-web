using SFA.DAS.Configuration.AzureTableStorage;
using System.Diagnostics.CodeAnalysis;
namespace SFA.DAS.AODP.Web.Extensions
{

    [ExcludeFromCodeCoverage]
    public static class LoadConfigurationExtensions
    {
        public static IConfigurationRoot LoadConfiguration(this IConfiguration configuration, IServiceCollection services, bool isDevelopment)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (configuration["EnvironmentName"] == "LOCAL")
            {
                configBuilder
                   .AddJsonFile("appsettings.json", true)
                   .AddJsonFile("appsettings.Development.json", true);
            }



            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            }
               );


            return configBuilder.Build();

        }
    }
}
