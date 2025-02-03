using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AODP.Web.DfeSignIn.Extensions;
using SFA.DAS.AODP.Web.DfeSignIn.Interfaces;
using SFA.DAS.AODPs.Web.DfeSignIn.Extensions;

namespace SFA.DAS.AODP.Web.DfeSignIn.Extensions
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