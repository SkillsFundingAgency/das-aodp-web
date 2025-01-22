using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using SFA.DAS.AODP.Domain.Services;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceRegistration(this IServiceCollection services)
        {
            services.AddHttpClient();
            //services.AddHttpClient<IApiClient, ApiClient>();
            //services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            services.AddTransient(typeof(IInternalApiClient<>), typeof(IInternalApiClient<>));
            services.AddTransient<IAodpApiClient<AodpApiConfiguration>, AodpApiClient>();
            return services;
        }
    }
}
