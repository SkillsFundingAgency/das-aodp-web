using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

                    // This was updated 

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler()
                    {
                        InboundClaimTypeMap = new Dictionary<string, string>(),
                        TokenLifetimeInMinutes = 90,
                        SetDefaultTimesOnTokenCreation = true
                    };
                    options.UseSecurityTokenValidator = true;

                    options.Events.OnRemoteFailure = c =>
                    {
                        try
                        {
                            if (c.Failure != null && c.Failure.Message.Contains("Correlation failed"))
                            {
                                c.Response.Redirect(redirectUrl);
                                c.HandleResponse();
                            }
                        }
                        catch (Exception ex)
                        {
                            var logger = new LoggerFactory().CreateLogger("OnRemoteFailure");
                            logger.LogError($"error occured :{ex.GetBaseException().Message}");
                        }
                        return Task.CompletedTask;
                    };

                    options.Events.OnSignedOutCallbackRedirect = c =>
                    {
                        try
                        {
                            c.Response.Cookies.Delete(authenticationCookieName); // delete the client cookie by given cookie name.
                            c.Response.Redirect(c.Options.SignedOutRedirectUri); // the path the authentication provider posts back after signing out.
                            c.HandleResponse();
                        }
                        catch (Exception ex)
                        {
                            var logger = new LoggerFactory().CreateLogger("Onsignedoutcallback");
                            logger.LogError($"error occured :{ex.GetBaseException().Message}");
                        }
                        return Task.CompletedTask;
                    };
                })
                .AddAuthenticationCookie(authenticationCookieName, signedOutCallbackPath, configuration["ResourceEnvironmentName"]);

            services
                .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                .Configure<IDfESignInService, IOptions<DfEOidcConfiguration>, ITicketStore>(
                    (options, dfeSignInService, config, ticketStore) =>
                    {
                            options.Events.OnTokenValidated = async ctx => await dfeSignInService.PopulateAccountClaims(ctx);
                    });
            services
                .AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
                .Configure<ITicketStore, DfEOidcConfiguration>((options, ticketStore, config) =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(config.LoginSlidingExpiryTimeOutInMinutes);
                    options.SlidingExpiration = true;
                    options.SessionStore = ticketStore;
                });
        }

    }
}