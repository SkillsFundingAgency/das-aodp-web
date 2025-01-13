using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SFA.DAS.AODP.Infrastructure.ApiClients;
using SFA.DAS.AODP.Infrastructure.ExampleData;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.LoadConfiguration(builder.Services, builder.Environment.IsDevelopment());

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensure session cookie is not accessible via JavaScript
    options.Cookie.IsEssential = true; // Required for non-essential cookies
});

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 100; // Limit memory to 100 MB
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // Frequency to scan expired items
});

var aodpAPIEndpoint = builder.Configuration.GetSection("AodpApiClientConfiguration:ApiBaseUrl").Value;
if (aodpAPIEndpoint != null)
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(aodpAPIEndpoint) });
}
else
{
    throw new Exception("Unable to set 'Telepurchase API Endpoint'");
}

builder.Services.AddSingleton<ICacheManager, CacheManager>();

builder.Services.AddControllers();

builder.Services
    .AddServiceRegistrations(configuration)
    .AddAodpApiClient(configuration, builder.Environment.IsDevelopment())
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


var app = builder.Build();

// Seed the data
using (var scope = app.Services.CreateScope())
{
    var cacheManager = scope.ServiceProvider.GetRequiredService<ICacheManager>();
    DataSeeder.Seed(cacheManager);
}

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
    .UseSession() // Enable session
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");
    });

app.MapControllers();

app.Run();
