using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.AODP.Authentication.Extensions
{
    public static class AddAndConfigureDfESignInAuthenticationExtension
    {
        public static void AddAndConfigureDfESignInAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            string authenticationCookieName,
            Type customServiceRole,
            string signedOutCallbackPath = "/signed-out",
            string redirectUrl = "")
        {
            services.AddServiceRegistration(configuration, customServiceRole);
            services.ConfigureDfESignInAuthentication(configuration, authenticationCookieName, signedOutCallbackPath, "/");
        }
    }
}