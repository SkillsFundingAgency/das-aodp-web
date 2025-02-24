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

        services.Configure<FormBuilderSettings>(configuration.GetSection("FormBuilderSettings"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<FormBuilderSettings>>().Value);

        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorageSettings"));
        services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<BlobStorageSettings>>().Value);
    }
}
