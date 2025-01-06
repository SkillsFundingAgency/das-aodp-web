using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.AODP.Infrastructure.ApiClients
{
    public static class ApiClientExtensions
    {
        public static IServiceCollection AddAodpApiClient(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            var config = configuration.GetRequiredSection("AodpApiClientConfiguration").Get<AodpApiClientConfiguration>()!;
            services.AddSingleton(config);

            services.AddTransient<IApiClientHelper, ApiClientHelper>();
            services.AddTransient<IAodpApiClient, AodpApiClient>();

            if (isDevelopment)
            {
                services.AddTransient<IAodpApiClientFactory, LocalAodpApiClientFactory>();
            }
            else
            {
                throw new NotImplementedException();
            }

            return services;

        }
    }
}