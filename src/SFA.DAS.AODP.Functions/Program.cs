using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.AODP.Infrastructure.Context;

var builder = FunctionsApplication.CreateBuilder(args);

// Load configuration from local.settings.json
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

// Log the connection string to verify
var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Program");
var connectionString = builder.Configuration["Values:DefaultConnection"];
logger.LogInformation("Connection String: {ConnectionString}", connectionString);

// Register ApplicationDbContext with the dependency injection container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["Values:DefaultConnection"]);
});

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

builder.Build().Run();
