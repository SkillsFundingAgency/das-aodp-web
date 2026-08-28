using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.Extensions
{
    public static class AddFileServiceExtensions
    {
        public static IServiceCollection AddFileService(this IServiceCollection services, StorageSettings storageSettings)
        {
            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(new Uri(storageSettings.ServiceUri));
            });

            services.AddScoped<IFileService, BlobStorageFileService>();
            return services;
        }
    }
}
