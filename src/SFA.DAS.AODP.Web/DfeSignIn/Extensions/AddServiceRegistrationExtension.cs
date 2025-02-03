using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using SFA.DAS.AODP.Web.DfeSignIn.Services;
using SFA.DAS.AODP.Web.DfeSignIn.Configuration;
using SFA.DAS.AODP.Web.DfeSignIn.Interfaces;
using SFA.DAS.AODP.Web.DfeSignIn.DfeSignInApi.Client;
using SFA.DAS.AODP.Web.DfeSignIn.DfeSignInApi.JWTHelpers;

namespace SFA.DAS.AODPs.Web.DfeSignIn.Extensions
{
    internal static class AddServiceRegistrationExtension
    {
        internal static void AddServiceRegistration(
            this IServiceCollection services,
            IConfiguration configuration,
            Type customServiceRole)
        {
            if (!configuration.GetSection(nameof(DfEOidcConfiguration)).GetChildren().Any())
            {
                throw new ArgumentException(
                    "Cannot find DfEOidcConfiguration in configuration. Please add a section called DfESignInOidcConfiguration with BaseUrl, ClientId and Secret properties.");
            }


            services.AddOptions();

            services.Configure<DfEOidcConfiguration>(configuration.GetSection(nameof(DfEOidcConfiguration)));



            services.AddSingleton(cfg => cfg.GetService<IOptions<DfEOidcConfiguration>>().Value);
            services.AddTransient(typeof(ICustomServiceRole), customServiceRole);
            services.AddTransient<IDfESignInService, DfESignInService>();
            services.AddHttpClient<IDFESignInAPIClient, DFESignInAPIClient>
                (
                    options => options.Timeout = TimeSpan.FromMinutes(30)
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(HttpClientRetryPolicy());
            services.AddTransient<ITokenDataSerializer, TokenDataSerializer>();
            services.AddTransient<ITokenBuilder, TokenBuilder>();
            services.AddSingleton<ITicketStore, AuthenticationTicketStore>();

            var connection = configuration.GetSection(nameof(DfEOidcConfiguration)).Get<DfEOidcConfiguration>();
            if (string.IsNullOrEmpty(connection.DfELoginSessionConnectionString))
            {
#if NETSTANDARD2_0
                services.AddMemoryCache();
#else
                services.AddDistributedMemoryCache();
#endif
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connection.DfELoginSessionConnectionString;
                });
            }
        }

        private static IAsyncPolicy<HttpResponseMessage> HttpClientRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}