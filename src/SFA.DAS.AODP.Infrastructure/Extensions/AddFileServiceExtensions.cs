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

            services.AddTransient<IFileService, BlobStorageFileService>();
            return services;

        }
    }
}
