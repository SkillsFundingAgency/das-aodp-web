using MediatR;
using SFA.DAS.AODP.Application.Behaviours;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.ApiClient;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Infrastructure.Extensions;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Helpers.Export;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Extensions.Startup;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddConfigurationOptions(configuration);

        services.AddSingleton(configuration);

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(GetFormVersionByIdQuery).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediatrExceptionHandlingBehaviour<,>));

        services.AddHttpClient<IApiClient, ApiClient>();

        services.AddValidators();

        services.AddFluentValidators();

        services.AddFileService(configuration);

        services.AddScoped<IUserHelperService, UserHelperService>();

        services.AddSingleton<IMessageFileValidationService, MessageFileValidationService>();
        services.AddScoped<IHtmlExportRenderer, HtmlExportRenderer>();
        services.AddScoped<IApplicationExportService, ApplicationExportService>();


        services.AddSingleton<FileUploadValidator>();

        services.AddSingleton<IFileStorageLocationPolicy, AzureBlobLocationPolicy>();

        services.AddTransient<IQualificationTimelineHistoryBuilder, QualificationTimelineHistoryBuilder>();

        services.AddScoped<ICsvFileReader, CsvFileReader>();

        return services;
    }
}
