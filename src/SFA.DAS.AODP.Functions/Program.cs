using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.AODP.Infrastructure.Context;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Retrieve the connection string from environment variables
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

        // Log connection string for debugging (optional, but useful during local development)
        Console.WriteLine($"Connection String: {connectionString}");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
    })
    .Build();

host.Run();
