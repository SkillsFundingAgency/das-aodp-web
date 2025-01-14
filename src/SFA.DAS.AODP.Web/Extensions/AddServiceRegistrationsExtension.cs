using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Infrastructure.Api;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.FAA.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddSingleton(configuration);

        services.AddSingleton(configuration.GetRequiredSection(nameof(AodpOuterApiSettings)).Get<AodpOuterApiSettings>()!);
        services.AddHttpClient<IApiClient, ApiClient>();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(TestQuery).Assembly));


        return services;
    }
}
