using Microsoft.Extensions.Configuration;
using SFA.DAS.ADPO.Models.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using System.Diagnostics.CodeAnalysis;
namespace SFA.DAS.ADPO.Common.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class LoadAzureTableStorageConfigurationExtensions
    {
        public static IConfigurationBuilder LoadAzureTableStorage(this IConfigurationBuilder builder, IConfiguration configuration)
        {
            var tableConfig = configuration.GetRequiredSection("AzureTableStorage").Get<AzureTableStorageConfiguration>()!;

            builder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = tableConfig.ConfigurationKeys?.Split(",");
                options.StorageConnectionString = tableConfig.StorageConnectionString;
                options.EnvironmentName = tableConfig.EnvironmentName;
                options.PreFixConfigurationKeys = false;

            }
                );

            return builder;
        }
    }
}
