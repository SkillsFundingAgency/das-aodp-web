﻿using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Constants;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses;
using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Extensions;
using SFA.DAS.AODP.Authentication.Interfaces;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using UserAccessResponse = SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses.UserAccessResponse;

[assembly: InternalsVisibleTo("SFA.DAS.DfESignIn.Auth.UnitTests")]

namespace SFA.DAS.AODP.Authentication.Services
{
    public class DfESignInService : IDfESignInService
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
                {
                    var userOganisationId = userOrganisation.Id;
                    await PopulateUserAccessClaims(ctx, userId, userOganisationId);
                    await PopulateUserOrganisationsClaims(ctx, userId, userOganisationId);
                }
                var displayName = $"{ctx.Principal.GetClaimValue(ClaimName.GivenName)} {ctx.Principal.GetClaimValue(ClaimName.FamilyName)}";

                ctx.HttpContext.Items.Add(ClaimsIdentity.DefaultNameClaimType, userId);
                ctx.HttpContext.Items.Add(CustomClaimsIdentity.DisplayName, displayName);

                ctx.Principal.Identities.First().AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, displayName));
                ctx.Principal.Identities.First().AddClaim(new Claim(CustomClaimsIdentity.DisplayName, displayName));
                ctx.Principal.Identities.First().AddClaim(new Claim(CustomClaimsIdentity.UkPrn, ukPrn));
            }
        }

        private async Task PopulateUserAccessClaims(TokenValidatedContext ctx, string userId, Guid userOrgId)
        {
            var response = await _DFESignInAPIClient.Get<UserAccessResponse>($"{_configuration.APIServiceUrl}/services/{_configuration.APIServiceId}/organisations/{userOrgId}/users/{userId}");

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
                    // Check if the custom service organisation type is set in client side if not use the default CustomClaimsIdentity.Service
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

        private async Task PopulateUserOrganisationsClaims(TokenValidatedContext ctx, string userId, Guid organisationId)
        {
            var response = await _DFESignInAPIClient.Get<List<UserOrganisationResponse>>($"{_configuration.APIServiceUrl}/users/{userId}/organisations");

            if (response != null)
            {
                var organisationDetails = response.Where(o => o.Id.ToLower() == organisationId.ToString()).Single();

                if (organisationDetails != null)
                {
                    var organisationClaims = new List<Claim>();
                    organisationClaims.Add(new Claim(ClaimName.OrganisationName, organisationDetails.Name ?? ""));
                    organisationClaims.Add(new Claim(ClaimName.UKPrn, organisationDetails.Ukprn ?? ""));
                    organisationClaims.Add(new Claim(ClaimName.LegalName, organisationDetails.LegalName ?? ""));
                    ctx?.Principal?.Identities.First().AddClaims(organisationClaims);
                }
            }
        }
    }
}
