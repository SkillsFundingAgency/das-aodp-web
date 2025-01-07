using Microsoft.OpenApi.Models;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration.LoadConfiguration(builder.Services, builder.Environment.IsDevelopment());

// Add services to the container.
builder.Services
    .AddServiceRegistrations(configuration)
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<TestQueryHandler>())
    .AddLogging()
    .AddDataProtectionKeys("das-aodp-api", configuration, builder.Environment.IsDevelopment())
    .AddHttpContextAccessor()
    .AddHealthChecks();

builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "AODP API", Version = "v1" });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app
    .UseHealthChecks("/ping")
    .UseHttpsRedirection()
    .UseAuthorization();

app.MapControllers();

app.Run();
