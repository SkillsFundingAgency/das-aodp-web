using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Web.Extensions;

public static class AddConfigurationOptionsExtension
{
    public static void AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<AodpOuterApiSettings>(configuration.GetSection("AodpOuterApiSettings"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<AodpOuterApiSettings>>().Value);
    }
}
