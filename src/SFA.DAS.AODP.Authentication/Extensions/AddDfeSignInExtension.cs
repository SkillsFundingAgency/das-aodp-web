using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.AODP.Authentication.Extensions
{
    public static class AddDfeSignInExtension
    {
        public static void AddDfeSignIn(
            this IServiceCollection services,
            IConfiguration configuration,
            string authenticationCookieName,
            Type customServiceRole,
            string signedOutCallbackPath = "/signed-out",
            string redirectUrl = "")
        {
            services.AddServiceRegistration(configuration, customServiceRole);
            var stubAuth = configuration["StubAuth"] ?? "false";
            if (stubAuth.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddStubAuthentication(authenticationCookieName, signedOutCallbackPath, configuration["ResourceEnvironmentName"]);
            }
            else
            {
                services.ConfigureDfESignInAuthentication(configuration, authenticationCookieName, signedOutCallbackPath, "/");
            }
        }
    }
}