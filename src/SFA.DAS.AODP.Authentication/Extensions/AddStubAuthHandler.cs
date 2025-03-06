using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Authentication.Constants;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SFA.DAS.AODP.Authentication.Extensions
{
    [ExcludeFromCodeCoverage]
    public class StubAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StubAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(options, logger, encoder)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "DfEStaffUser"),
                new Claim(CustomClaimsIdentity.DisplayName, "DfE Staff User"),
                new Claim(CustomClaimsIdentity.Service, "QFAdmin"),
                new Claim(CustomClaimsIdentity.UkPrn, "10000001"),
                new Claim("organisation", "{\"id\":\"71A7429D-1875-4CDD-8FEF-A89676E661A8\"}"),
                new Claim("roleName", "qfau_user_approver"),
                new Claim("roleName", "qfau_user_reviewer"),
                new Claim("roleName", "ifate_user_reviewer"),
                new Claim("roleName", "ofqual_user_reviewer"),
                new Claim("rolenumericid", "22328"),
                new Claim("rolecode", "QFAdmin"),
                new Claim("roleId", "03ff5868-31cd-453e-ae9f-fad5f101124d"),
                new Claim("email", "tester@education.gov.uk"),                
                new Claim("roleName", "qfau_admin_form_editor"),
                new Claim("roleName", "ifate_admin_form_editor"),
                new Claim("roleName", "qfau_admin_data_importer"),
                new Claim("roleName", "ao_user"),
            };

            var identity = new ClaimsIdentity(claims, "Provider-stub");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Provider-stub");

            var result = AuthenticateResult.Success(ticket);

            _httpContextAccessor.HttpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType, "DfEStaffUser");
            _httpContextAccessor.HttpContext.Items.Add(CustomClaimsIdentity.DisplayName, "DfE Staff User");

            return result;
        }
    }
}
