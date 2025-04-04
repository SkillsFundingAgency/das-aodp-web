﻿//using System.Security.Claims;
//using AutoFixture.NUnit3;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Options;
//using Moq;
//using Newtonsoft.Json;
//using SFA.DAS.AODP.Authentication.Configuration;
//using SFA.DAS.AODP.Authentication.Constants;
//using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
//using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
//using SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses;
//using SFA.DAS.AODP.Authentication.Enums;
//using SFA.DAS.AODP.Authentication.Interfaces;
//using SFA.DAS.AODP.Authentication.Services;
//using SFA.DAS.Testing.AutoFixture;

//namespace SFA.DAS.DfESignIn.Auth.UnitTests.Services
//{
//    public class DfESignInServiceTest
//    {
//        [Fact, MoqAutoData]
//        public async Task Then_The_Claims_Are_Populated_For_Users(
//            string userId,
//            Organisation organisation,
//            DfEOidcConfiguration config,
//            UserAccessResponse response,
//            [Frozen] Mock<IDFESignInAPIClient> apiHelper,
//            [Frozen] Mock<IOptions<DfEOidcConfiguration>> configuration,
//            [Frozen] Mock<ICustomServiceRole> customServiceRole)
//        {
//            var tokenValidatedContext = ArrangeTokenValidatedContext(userId, organisation, string.Empty);
//            apiHelper.Setup(x => x.Get<UserAccessResponse>($"{config.APIServiceUrl}/services/{config.APIServiceId}/organisations/{organisation.Id}/users/{userId}")).ReturnsAsync(response);
//            configuration.Setup(c => c.Value).Returns(config);
//            customServiceRole.Setup(role => role.RoleClaimType).Returns(CustomClaimsIdentity.Service);
//            customServiceRole.Setup(role => role.RoleValueType).Returns(CustomServiceRoleValueType.Name);
//            var service = new DfESignInService(configuration.Object, apiHelper.Object, customServiceRole.Object);

//            await service.PopulateAccountClaims(tokenValidatedContext);

//            var actualClaims = tokenValidatedContext.Principal?.Identities.First().Claims.ToList();
//            actualClaims?.First(c => c.Type.Equals(CustomClaimsIdentity.UkPrn)).Value.Should().Be(organisation.UkPrn.ToString());
//            actualClaims?.First(c => c.Type.Equals(ClaimsIdentity.DefaultNameClaimType)).Value.Should().Be("Test Tester");
//            actualClaims?.First(c => c.Type.Equals(ClaimName.Sub)).Value.Should().Be(userId);
//            actualClaims?.First(c => c.Type.Equals(CustomClaimsIdentity.DisplayName)).Value.Should().Be("Test Tester");
//        }

//        [Fact, MoqAutoData]
//        public async Task Then_The_Organisations_Are_Added_To_The_Claims(
//            string userId,
//            Organisation organisation,
//            DfEOidcConfiguration config,
//            UserAccessResponse response,
//            [Frozen] Mock<IDFESignInAPIClient> apiHelper,
//            [Frozen] Mock<IOptions<DfEOidcConfiguration>> configuration,
//            [Frozen] Mock<ICustomServiceRole> customServiceRole)
//        {
//            var tokenValidatedContext = ArrangeTokenValidatedContext(userId, organisation, string.Empty);
//            response.Roles = response.Roles.Select(c => { c.Status.Id = (int)RoleStatus.Active; return c; }).ToList();
//            apiHelper.Setup(x => x.Get<UserAccessResponse>($"{config.APIServiceUrl}/services/{config.APIServiceId}/organisations/{organisation.Id}/users/{userId}")).ReturnsAsync(response);
//            configuration.Setup(c => c.Value).Returns(config);
//            customServiceRole.Setup(role => role.RoleClaimType).Returns(CustomClaimsIdentity.Service);
//            customServiceRole.Setup(role => role.RoleValueType).Returns(CustomServiceRoleValueType.Name);
//            var service = new DfESignInService(configuration.Object, apiHelper.Object, customServiceRole.Object);

//            await service.PopulateAccountClaims(tokenValidatedContext);

//            var claims = tokenValidatedContext.Principal?.Identities.Last().Claims.ToList();
//            if (claims != null)
//            {
//                claims.Where(x => x.Type.Equals(ClaimName.RoleCode)).Select(c => c.Value).Should()
//                    .BeEquivalentTo(response.Roles.Select(c => c.Code).ToList());
//                claims.Where(x => x.Type.Equals(ClaimName.RoleId)).Select(c => c.Value).Should()
//                    .BeEquivalentTo(response.Roles.Select(c => c.Id.ToString()).ToList());
//                claims.Where(x => x.Type.Equals(ClaimName.RoleName)).Select(c => c.Value).Should()
//                    .BeEquivalentTo(response.Roles.Select(c => c.Name).ToList());
//                claims.Where(x => x.Type.Equals(ClaimName.RoleNumericId)).Select(c => c.Value).Should()
//                    .BeEquivalentTo(response.Roles.Select(c => c.NumericId.ToString()).ToList());
//                claims.Where(x => x.Type.Equals(CustomClaimsIdentity.Service)).Select(c => c.Value).Should()
//                    .BeEquivalentTo(response.Roles.Select(c => c.Name).ToList());
//            }
//        }

//        private static TokenValidatedContext ArrangeTokenValidatedContext(string userId, Organisation organisation, string emailAddress)
//        {
//            return new TokenValidatedContext(
//                new DefaultHttpContext(),
//                new AuthenticationScheme(",", "", typeof(TestAuthHandler)),
//                new OpenIdConnectOptions(), Mock.Of<ClaimsPrincipal>(),
//                new AuthenticationProperties())
//            {
//                Principal = new ClaimsPrincipal(
//                    new ClaimsIdentity(
//                    new ClaimsIdentity(
//                    new List<Claim>
//                {
//                    new(ClaimName.Sub, userId),
//                    new(ClaimTypes.Email, emailAddress),
//                    new(ClaimName.Organisation, JsonConvert.SerializeObject(organisation)),
//                    new(ClaimName.FamilyName, "Tester"),
//                    new(ClaimName.GivenName, "Test")
//                })))
//            };
//        }


//        private class TestAuthHandler : IAuthenticationHandler
//        {
//            public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
//            {
//                throw new NotImplementedException();
//            }

//            public Task<AuthenticateResult> AuthenticateAsync()
//            {
//                throw new NotImplementedException();
//            }

//            public Task ChallengeAsync(AuthenticationProperties? properties)
//            {
//                throw new NotImplementedException();
//            }

//            public Task ForbidAsync(AuthenticationProperties? properties)
//            {
//                throw new NotImplementedException();
//            }
//        }
//    }
//}
