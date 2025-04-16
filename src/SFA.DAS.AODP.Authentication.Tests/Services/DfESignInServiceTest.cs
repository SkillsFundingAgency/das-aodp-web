using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Constants;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses;
using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Interfaces;
using SFA.DAS.AODP.Authentication.Services;

namespace SFA.DAS.DfESignIn.Auth.UnitTests.Services
{
    public class DfESignInServiceTest
    {
        [Fact]
        public async Task Then_The_Claims_Are_Populated_For_Users()

        {
            string userId = "123";
            Organisation organisation = new() { UkPrn = 123 };
            DfEOidcConfiguration config = new() { ClientId = "123" };
            UserAccessResponse response = new() { Roles = new List<Role> { new() { Id = new Guid(), Code = "123", Name = "Test", Status = new() { Id = 1 } } } };
            ;
            Mock<IDFESignInAPIClient> apiHelper = new();
            Mock<IOptions<DfEOidcConfiguration>> configuration = new();
            Mock<ICustomServiceRole> customServiceRole = new() { };

            var tokenValidatedContext = ArrangeTokenValidatedContext(userId, organisation, string.Empty);
            apiHelper.Setup(x => x.Get<UserAccessResponse>($"{config.APIServiceUrl}/services/{config.APIServiceId}/organisations/{organisation.Id}/users/{userId}")).ReturnsAsync(response);
            configuration.Setup(c => c.Value).Returns(config);
            customServiceRole.Setup(role => role.RoleClaimType).Returns(CustomClaimsIdentity.Service);
            customServiceRole.Setup(role => role.RoleValueType).Returns(CustomServiceRoleValueType.Name);
            var service = new DfESignInService(configuration.Object, apiHelper.Object, customServiceRole.Object);

            await service.PopulateAccountClaims(tokenValidatedContext);

            var actualClaims = tokenValidatedContext.Principal?.Identities.First().Claims.ToList();
            Assert.Equal(organisation.UkPrn.ToString(), actualClaims?.First(c => c.Type.Equals(CustomClaimsIdentity.UkPrn)).Value);
            Assert.Equal("Test Tester", actualClaims?.First(c => c.Type.Equals(ClaimsIdentity.DefaultNameClaimType)).Value);
            Assert.Equal(userId, actualClaims?.First(c => c.Type.Equals(ClaimName.Sub)).Value);
            Assert.Equal("Test Tester", actualClaims?.First(c => c.Type.Equals(CustomClaimsIdentity.DisplayName)).Value);
        }

        [Fact]
        public async Task Then_The_Organisations_Are_Added_To_The_Claims()

        {
            string userId = "123";
            Organisation organisation = new() { UkPrn = 123 };
            DfEOidcConfiguration config = new() { ClientId = "123" };
            UserAccessResponse response = new() { Roles = new List<Role> { new() { Id = new Guid(), Code = "123", Name = "Test", Status = new() { Id = 1 } } } };
            ;
            Mock<IDFESignInAPIClient> apiHelper = new();
            Mock<IOptions<DfEOidcConfiguration>> configuration = new();
            Mock<ICustomServiceRole> customServiceRole = new() { };

            var tokenValidatedContext = ArrangeTokenValidatedContext(userId, organisation, string.Empty);
            response.Roles = response.Roles.Select(c => { c.Status.Id = (int)RoleStatus.Active; return c; }).ToList();
            apiHelper.Setup(x => x.Get<UserAccessResponse>($"{config.APIServiceUrl}/services/{config.APIServiceId}/organisations/{organisation.Id}/users/{userId}")).ReturnsAsync(response);
            configuration.Setup(c => c.Value).Returns(config);
            customServiceRole.Setup(role => role.RoleClaimType).Returns(CustomClaimsIdentity.Service);
            customServiceRole.Setup(role => role.RoleValueType).Returns(CustomServiceRoleValueType.Name);
            var service = new DfESignInService(configuration.Object, apiHelper.Object, customServiceRole.Object);

            await service.PopulateAccountClaims(tokenValidatedContext);

            var claims = tokenValidatedContext.Principal?.Identities.Last().Claims.ToList();
            if (claims != null)
            {
                Assert.Equal(claims.Where(x => x.Type.Equals(ClaimName.RoleCode)).Select(c => c.Value), response.Roles.Select(c => c.Code).ToList());
                Assert.Equal(claims.Where(x => x.Type.Equals(ClaimName.RoleId)).Select(c => c.Value), response.Roles.Select(c => c.Id.ToString()).ToList());
                Assert.Equal(claims.Where(x => x.Type.Equals(ClaimName.RoleName)).Select(c => c.Value), response.Roles.Select(c => c.Name).ToList());
                Assert.Equal(claims.Where(x => x.Type.Equals(ClaimName.RoleNumericId)).Select(c => c.Value), response.Roles.Select(c => c.NumericId.ToString()).ToList());
                Assert.Equal(claims.Where(x => x.Type.Equals(CustomClaimsIdentity.Service)).Select(c => c.Value), response.Roles.Select(c => c.Name).ToList());
            }
        }

        private static TokenValidatedContext ArrangeTokenValidatedContext(string userId, Organisation organisation, string emailAddress)
        {
            return new TokenValidatedContext(
                new DefaultHttpContext(),
                new AuthenticationScheme(",", "", typeof(TestAuthHandler)),
                new OpenIdConnectOptions() { ClientId = "1234" }, Mock.Of<ClaimsPrincipal>(),
                new AuthenticationProperties())
            {
                Principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                    new ClaimsIdentity(
                    new List<Claim>
                {
                    new(ClaimName.Sub, userId),
                    new(ClaimTypes.Email, emailAddress),
                    new(ClaimName.Organisation, JsonConvert.SerializeObject(organisation)),
                    new(ClaimName.FamilyName, "Tester"),
                    new(ClaimName.GivenName, "Test")
                })))
            };
        }


        private class TestAuthHandler : IAuthenticationHandler
        {
            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
            {
                throw new NotImplementedException();
            }

            public Task<AuthenticateResult> AuthenticateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ChallengeAsync(AuthenticationProperties? properties)
            {
                throw new NotImplementedException();
            }

            public Task ForbidAsync(AuthenticationProperties? properties)
            {
                throw new NotImplementedException();
            }
        }
    }
}
