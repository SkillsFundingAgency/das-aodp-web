using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.ADPO.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddSingleton(configuration);
        return services;
    }
}
