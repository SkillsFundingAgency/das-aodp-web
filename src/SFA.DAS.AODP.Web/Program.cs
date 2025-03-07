using GovUk.Frontend.AspNetCore;
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

        builder.Services.Configure<FormOptions>(options =>
        {
            // Set the limit to 256 MB
            options.MultipartBodyLengthLimit = 268435456;
        });

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
                .UseHsts()
                .UseExceptionHandler("/Home/Error");
        }
        app.Use(async (context, next) =>
        {
            Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
            await next.Invoke();
        });

        app
            .UseHealthChecks("/ping")
            .UseHttpsRedirection()
            .UseStaticFiles()
            .UseCookiePolicy()
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
        endpoints.MapDefaultControllerRoute();

        endpoints.MapControllerRoute(name: "ReviewHomeLandingPage",
                                     pattern: "Review/Home/{action=Index}/{id?}",
                                     defaults: new { area = "Review", controller = "Home" });

        endpoints.MapControllerRoute(name: "ReviewDefault",
                                     pattern: "review/{controller=review}/{action=Index}/{id?}",
                                     defaults: new { area = "review" }).
                                     RequireAuthorization(PolicyConstants.IsReviewUser);

        endpoints.MapAreaControllerRoute(name: "AdminHomeLandingPage",
                               areaName: "Admin",
                               pattern: "{area:exists}/Home/{action=Index}/{id?}",
                                defaults: new { controller = "Home" }).AllowAnonymous();

        endpoints.MapAreaControllerRoute(name: "AdminDefaultForImport",
                        areaName: "Admin",
                        pattern: "Admin/Import/{action=Index}/{id?}",
                        defaults: new { area = "Admin", controller = "Import" }).RequireAuthorization(PolicyConstants.IsAdminImportUser);

        endpoints.MapControllerRoute(name: "AdminDefaultForForms",
                                         pattern: "Admin/{controller=Dashboard}/{action=index}/{id?}",
                                         defaults: new { area = "Admin" }).RequireAuthorization(PolicyConstants.IsAdminFormsUser);

        endpoints.MapControllerRoute(name: "ApplyHome",
                                  pattern: "Apply/Home/{action=Index}/{id?}",
                                  defaults: new { area = "Apply", controller = "Home" }).AllowAnonymous();

        endpoints.MapAreaControllerRoute(name: "Apply",
                                       areaName: "Apply",
                                       pattern: "{area:exists}/{controller=Apply}/{action=Index}/{id?}").RequireAuthorization(PolicyConstants.IsApplyUser);
    }
}