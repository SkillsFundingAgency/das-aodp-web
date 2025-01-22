using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Common.Configuration;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Web.Extensions;

public static class AddConfigurationOptionsExtension
{
    public static void AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<AzureActiveDirectoryConfiguration>(configuration.GetSection("AzureAd"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<AzureActiveDirectoryConfiguration>>().Value);

        services.Configure<AodpApiConfiguration>(configuration.GetSection("AodpInnerApiConfiguration"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<AodpApiConfiguration>>().Value);
    }
}
