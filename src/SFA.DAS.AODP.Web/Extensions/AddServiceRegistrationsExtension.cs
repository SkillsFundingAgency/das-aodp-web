using SFA.DAS.AODP.Application.AutoMapper.Profiles;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.ApiClient;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddConfigurationOptions(configuration);

        services.AddSingleton(configuration);

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(GetFormVersionByIdQuery).Assembly));
        services.AddAutoMapper(typeof(AutoMapperProfile));

        services.AddHttpClient<IApiClient, ApiClient>();

        return services;
    }
}
