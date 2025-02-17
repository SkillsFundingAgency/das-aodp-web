using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;

        }
    }
}
