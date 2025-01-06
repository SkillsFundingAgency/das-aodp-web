using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ADPO.Infrastructure.ApiClients;
using SFA.DAS.ADPO.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.LoadConfiguration(builder.Services, builder.Environment.IsDevelopment());

builder.Services
    .AddServiceRegistrations(configuration)
    .AddAdpoApiClient(configuration, builder.Environment.IsDevelopment())
    .AddLogging()
    .AddDataProtectionKeys("das-adpo-web", configuration, builder.Environment.IsDevelopment())
    .AddHttpContextAccessor()
    .AddHealthChecks();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ADPO.Session";
});

builder.Services
     .AddMvc(options =>
     {
         options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
     });


var app = builder.Build();

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
    });

app.Run();
