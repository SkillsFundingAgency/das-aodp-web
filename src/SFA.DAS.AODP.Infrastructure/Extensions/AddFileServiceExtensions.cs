using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Infrastructure.Extensions
{
    public static class AddFileServiceExtensions
    {
        public static IServiceCollection AddFileService(this IServiceCollection services, IConfiguration configuration)
        {
            var primaryConnection = configuration.GetValue<string>("BlobStorageSettings:ConnectionString");
            var importConnection = configuration.GetValue<string>("ImportBlobStorageSettings:ConnectionString");

            services.AddAzureClients(builder =>
            {
                if (!string.IsNullOrWhiteSpace(primaryConnection))
                {
                    builder.AddBlobServiceClient(primaryConnection).WithName("default");
                }

                if (!string.IsNullOrWhiteSpace(importConnection))
                {
                    builder.AddBlobServiceClient(importConnection).WithName("import");
                }
            });

            // Provide the "default" client as the BlobServiceClient constructor parameter used elsewhere.
            if (!string.IsNullOrWhiteSpace(primaryConnection))
            {
                services.AddSingleton(provider =>
                    provider.GetRequiredService<IAzureClientFactory<BlobServiceClient>>().CreateClient("default"));
            }

            services.AddScoped<IFileService, BlobStorageFileService>();
            return services;
        }
    }
}
