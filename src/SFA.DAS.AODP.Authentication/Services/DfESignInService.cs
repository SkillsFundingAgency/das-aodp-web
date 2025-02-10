using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Constants;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Extensions;
using SFA.DAS.AODP.Authentication.Interfaces;
using System.Runtime.CompilerServices;
using System.Security.Claims;

[assembly: InternalsVisibleTo("SFA.DAS.DfESignIn.Auth.UnitTests")]

namespace SFA.DAS.AODP.Authentication.Services
{
    internal class DfESignInService : IDfESignInService
    {
        private readonly DfEOidcConfiguration _configuration;
        private readonly IDFESignInAPIClient _DFESignInAPIClient;
        private readonly ICustomServiceRole _customServiceRole;

        public DfESignInService(
            IOptions<DfEOidcConfiguration> configuration,
            IDFESignInAPIClient apiHelper,
            ICustomServiceRole customServiceRole)
        {
            _configuration = configuration.Value;
            _DFESignInAPIClient = apiHelper;
            _customServiceRole = customServiceRole;
        }

        public async Task PopulateAccountClaims(TokenValidatedContext ctx)
        {
            var userOrganisation = JsonConvert.DeserializeObject<Organisation>
            (
                ctx.Principal.GetClaimValue(ClaimName.Organisation)
            );

            if (userOrganisation != null && ctx.Principal != null)
            {
                var userId = ctx.Principal.GetClaimValue(ClaimName.Sub);
                var ukPrn = userOrganisation.UkPrn?.ToString() ?? "0";

                if (userId != null)
                    await PopulateUserAccessClaims(ctx, userId, Convert.ToString(userOrganisation.Id));

                var displayName = $"{ctx.Principal.GetClaimValue(ClaimName.GivenName)} {ctx.Principal.GetClaimValue(ClaimName.FamilyName)}";

                ctx.HttpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType, userId);
                ctx.HttpContext.Items.Add(CustomClaimsIdentity.DisplayName, displayName);

                ctx.Principal.Identities.First().AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, displayName));
                ctx.Principal.Identities.First().AddClaim(new Claim(CustomClaimsIdentity.DisplayName, displayName));
                ctx.Principal.Identities.First().AddClaim(new Claim(CustomClaimsIdentity.UkPrn, ukPrn));
            }
        }

        //private async Task PopulateOrganisations(TokenValidatedContext ctx, string userId)
        //{
        //    var response = await _DFESignInAPIClient.Get<ApiServiceResponse>($"{_configuration.APIServiceUrl}/services/{_configuration.APIServiceId}/organisations/{userOrgId}/users/{userId}");

        //}

        private async Task PopulateUserAccessClaims(TokenValidatedContext ctx, string userId, string userOrgId)
        {
            var response = await _DFESignInAPIClient.Get<ApiServiceResponse>($"{_configuration.APIServiceUrl}/services/{_configuration.APIServiceId}/organisations/{userOrgId}/users/{userId}");

            if (response != null)
            {
                var roleClaims = new List<Claim>();

                // Iterate the roles which are of only active status.
                foreach (var role in response.Roles.Where(role => role.Status.Id.Equals((int)RoleStatus.Active)))
                {
                    roleClaims.Add(new Claim(ClaimName.RoleCode, role.Code, ClaimTypes.Role, ctx.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimName.RoleId, role.Id.ToString(), ClaimTypes.Role, ctx.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimName.RoleName, role.Name, ClaimTypes.Role, ctx.Options.ClientId));
                    roleClaims.Add(new Claim(ClaimName.RoleNumericId, role.NumericId.ToString(), ClaimTypes.Role, ctx.Options.ClientId));

                    // Add to initial identity
                    // Check if the custom service role type is set in client side if not use the default CustomClaimsIdentity.Service
                    ctx.Principal?.Identities
                        .First()
                        .AddClaim(
                            new Claim(
                                type: _customServiceRole.RoleClaimType ?? CustomClaimsIdentity.Service,
                                value: _customServiceRole.RoleValueType.Equals(CustomServiceRoleValueType.Name)
                                    ? role.Name
                                    : role.Code));
                }
                ctx?.Principal?.Identities.First().AddClaims(roleClaims);
            }
        }
    }
}
