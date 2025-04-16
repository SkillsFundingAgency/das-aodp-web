using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Authentication.Extensions;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Extensions.Startup;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration.LoadConfiguration(builder.Services, builder.Environment.IsDevelopment());

        builder.Services
            .AddServiceRegistrations(configuration)
            .AddAuthorization(options =>
            {
                options.AddPolicy(PolicyConstants.IsReviewUser, policy => policy.RequireRole(RoleConstants.QFAUApprover, RoleConstants.QFAUReviewer, RoleConstants.IFATEReviewer, RoleConstants.OFQUALReviewer));
                options.AddPolicy(PolicyConstants.IsInternalReviewUser, policy => policy.RequireRole(RoleConstants.QFAUApprover, RoleConstants.QFAUReviewer));
                options.AddPolicy(PolicyConstants.IsApplyUser, policy => policy.RequireRole(RoleConstants.AOApply));
                options.AddPolicy(PolicyConstants.IsAdminFormsUser, policy => policy.RequireRole(RoleConstants.QFAUFormBuilder, RoleConstants.IFATEFormBuilder));
                options.AddPolicy(PolicyConstants.IsAdminImportUser, policy => policy.RequireRole(RoleConstants.QFAUImport));
            }
            )
            .AddGovUkFrontend()
            .AddLogging()
            .AddDistributedCache(configuration, builder.Environment.IsDevelopment())
            .AddDataProtectionKeys("das-aodp-web", configuration, builder.Environment.IsDevelopment())
            .AddHttpContextAccessor()
            .AddHealthChecks();

        builder.Services.AddDfeSignIn(configuration, "SFA.DAS.AODP.Web", typeof(CustomServiceRole), "/signout", "signins");

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".AODP.Session";
        });

        builder.Services
             .AddMvc(options =>
             {
                 options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
             });

        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        builder.Services.AddApplicationInsightsTelemetry();

        //builder.Services.Configure<FormOptions>(options =>
        //{
        //    // Set the limit to 100 MB for form data
        //    options.MultipartBodyLengthLimit = 104857600;
        //});

        var app = builder.Build();

        app.UseStatusCodePagesWithRedirects("/Error/{0}");
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app
                .UseExceptionHandler("/Home/Error")
                .UseHsts();
        }

        app
            .UseHealthChecks("/ping")
            .UseHttpsRedirection()
            .UseStaticFiles()
            .UseCookiePolicy()
            .UseWhen(context => context.Request.ContentType != null && context.Request.ContentType.StartsWith("multipart/form-data"), appBuilder =>
            {
                appBuilder.Use((context, next) =>
                {
                    // Set the maximum request body size to 110 MB (115343360 bytes).
                    // This is 10% larger than the MultipartBodyLengthLimit (100 MB) to provide a buffer for request overhead.
                    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 115343360; 
                    return next.Invoke();
                });
            })
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseSession()
            .UseEndpoints(endpoints =>
            {
                AddRoutes(endpoints);

            });
        app.Run();
    }

    private static void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAreaControllerRoute(name: "Review",
                                       areaName: "Review",
                                       pattern: "Review",
                                       defaults: new { area = "Review", controller = "Home", action = "Index" });


        endpoints.MapAreaControllerRoute(name: "Apply",
                                       areaName: "Apply",
                                       pattern: "Apply",
                                       defaults: new { area = "Apply", controller = "Applications", action = "Index" });

        endpoints.MapAreaControllerRoute(name: "Admin",
                               areaName: "Admin",
                               pattern: "Admin",
                               defaults: new { area = "Admin", controller = "Home", action = "Index" }).AllowAnonymous();

        endpoints.MapControllerRoute(name: "AdminDefaultForImport",
                      pattern: "Admin/Import/{action=Index}/{id?}",
                      defaults: new { area = "Admin", controller = "Import" }).RequireAuthorization(PolicyConstants.IsAdminImportUser);

        endpoints.MapControllerRoute(name: "AdminDefaultForOutput",
                    pattern: "Admin/OutputFile/{action=Index}/{id?}",
                    defaults: new { area = "Admin", controller = "OutputFile" }).RequireAuthorization(PolicyConstants.IsAdminImportUser);

        endpoints.MapControllerRoute(name: "AdminDefaultForForms",
                    pattern: "Admin/Forms/{action=index}/{id?}",
                    defaults: new { area = "Admin", controller = "Forms" }).RequireAuthorization(PolicyConstants.IsAdminFormsUser);


        endpoints.MapDefaultControllerRoute();
    }
}