using Microsoft.AspNetCore.DataProtection;
using SFA.DAS.ADPO.Web.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.ADPO.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class AddDataProtectionExtensions
{
    private const string ApplicationName = "das-adpo";

    public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services
                .AddDataProtection()
                .SetApplicationName(ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "keys")));

            services.AddDistributedMemoryCache();
        }
        else
        {
            throw new NotImplementedException("Need to implement persistance to Redis");
        }

        return services;
    }
}
