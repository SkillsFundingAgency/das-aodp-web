using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.AODP.Authentication.Extensions
{
    public static class ConfigureStubAuthenticationExtension
    {
        public static void AddStubAuthentication(
            this IServiceCollection services,
            string authenticationCookieName,
            string signedOutCallbackPath,
            string resourceEnvironmentName)
        {
            services
                .AddAuthentication(sharedOptions =>
                    {
                        sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                .AddScheme<AuthenticationSchemeOptions, StubAuthHandler>(authenticationCookieName, _ => { })
                .AddCookie(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Events.OnSigningOut = c =>
                    {
                        c.Response.Cookies.Delete(authenticationCookieName);
                        c.Response.Redirect("/");
                        return Task.CompletedTask;
                    };
                });

            services.AddAuthentication(authenticationCookieName).AddAuthenticationCookie(authenticationCookieName, signedOutCallbackPath, resourceEnvironmentName);
        }
    }
}
