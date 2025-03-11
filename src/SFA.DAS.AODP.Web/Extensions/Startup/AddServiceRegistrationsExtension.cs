using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.ApiClient;
using SFA.DAS.AODP.Infrastructure.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions.Startup;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddConfigurationOptions(configuration);

        services.AddSingleton(configuration);

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(GetFormVersionByIdQuery).Assembly));

        services.AddHttpClient<IApiClient, ApiClient>();

        services.AddValidators();

        services.AddFileService(configuration);

        services.AddScoped<IUserHelperService, UserHelperService>();

        return services;
    }
}
