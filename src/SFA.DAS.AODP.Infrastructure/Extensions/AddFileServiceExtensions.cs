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
            services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration.GetValue<string>("BlobStorageSettings:ConnectionString"));
            });

            var importConnection = configuration.GetValue<string>("ImportBlobStorageSettings:ConnectionString");
            if (!string.IsNullOrWhiteSpace(importConnection))
            {
                services.AddSingleton<IImportBlobServiceClient>(_ => new ImportBlobServiceClient(new BlobServiceClient(importConnection)));
            }

            services.AddTransient<IFileService, BlobStorageFileService>();
            return services;

        }
    }
}
