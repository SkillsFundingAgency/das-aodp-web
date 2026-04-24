using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Infrastructure.Extensions
{
    public static class AddFileServiceExtensions
    {
        public static IServiceCollection AddFileService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var primaryConnection =
                configuration.GetValue<string>("BlobStorageSettings:ConnectionString");

            if (string.IsNullOrWhiteSpace(primaryConnection))
            {
                throw new InvalidOperationException(
                    "BlobStorageSettings:ConnectionString is not configured");
            }

            services.AddSingleton(new BlobServiceClient(primaryConnection));
            services.AddScoped<IFileService, BlobStorageFileService>();

            return services;
        }
    }
}
