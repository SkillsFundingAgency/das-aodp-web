using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Constants;
using SFA.DAS.AODP.Authentication.Services;

namespace SFA.DAS.AODP.Authentication.Extensions
{
    internal static class ConfigureDfESignInAuthenticationExtension
    {
        internal static void ConfigureDfESignInAuthentication(
            this IServiceCollection services,
            IConfiguration configuration,
            string authenticationCookieName,
            string signedOutCallbackPath,
            string redirectUrl)
        {
            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = configuration["DfEOidcConfiguration:BaseUrl"];
                    options.ClientId = configuration[$"DfEOidcConfiguration:ClientId"];
                    options.ClientSecret = configuration[$"DfEOidcConfiguration:Secret"];
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SignedOutRedirectUri = redirectUrl;
                    options.SignedOutCallbackPath = new PathString(RoutePath.OidcSignOut); // the path the authentication provider posts back after signing out.
                    options.CallbackPath = new PathString(RoutePath.OidcSignIn); // the path the authentication provider posts back when authenticating.
                    options.ResponseType = "code";
                    var scopes = configuration["DfEOidcConfiguration:Scopes"].Split(' ');
                    options.Scope.Clear();
                    foreach (var scope in scopes)
                    {
                        options.Scope.Add(scope);
                    }
                    options.SaveTokens = true;

                })
                 .AddAuthenticationCookie(authenticationCookieName, signedOutCallbackPath, configuration["DfEOidcConfiguration:ResourceEnvironmentName"]);
        }
    }
}