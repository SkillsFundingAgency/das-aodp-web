using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.ADPO.Infrastructure.ApiClients
{
    public static class ApiClientExtensions
    {
        public static IServiceCollection AddAdpoApiClient(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            var config = configuration.GetRequiredSection("AdpoApiClientConfiguration").Get<AdpoApiClientConfiguration>()!;
            services.AddSingleton(config);

            services.AddTransient<IApiClientHelper, ApiClientHelper>();
            services.AddTransient<IAdpoApiClient, AdpoApiClient>();

            if (isDevelopment)
            {
                services.AddTransient<IAdpoApiClientFactory, LocalAdpoApiClientFactory>();
            }
            else
            {
                throw new NotImplementedException();
            }

            return services;

        }
    }
}