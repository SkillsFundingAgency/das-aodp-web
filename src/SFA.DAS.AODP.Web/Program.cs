using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.LoadConfiguration(builder.Services, builder.Environment.IsDevelopment());

builder.Services
    .AddServiceRegistrations(configuration)
    .AddGovUkFrontend()
    .AddLogging()
    .AddDataProtectionKeys("das-aodp-web", configuration, builder.Environment.IsDevelopment())
    .AddHttpContextAccessor()
    .AddHealthChecks();


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

//builder.Services.AddMemoryCache(options =>
//{
//    options.SizeLimit = 1024 * 1024 * 100; // Limit memory to 100 MB
//    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Frequency to scan expired items
//});

//builder.Services.AddScoped<ICacheManager, CacheManager>();
//builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(CachedGenericRepository<>));

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

var app = builder.Build();

//// Seed the data
//using (var scope = app.Services.CreateScope())
//{
//    var cacheManager = scope.ServiceProvider.GetRequiredService<ICacheManager>();
//    DataSeeder.Seed(cacheManager);
//}

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

app
    .UseHealthChecks("/ping")
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseCookiePolicy()
    .UseRouting()
    .UseSession()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
          );
    });

app.Run();
