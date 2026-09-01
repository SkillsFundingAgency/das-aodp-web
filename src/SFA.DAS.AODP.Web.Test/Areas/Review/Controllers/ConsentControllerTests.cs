using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Consent;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class ConsentControllerTests
    {
        private static ConsentController CreateController(bool hasConsentCookie = false)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("rolecode", "ao_user"),
                new Claim("email", "ao.user@test.com")
            }, "test"));

            if (hasConsentCookie)
            {
                httpContext.Request.Headers.Cookie = $"{ConsentController.GetConsentCookieName(httpContext)}=true";
            }

            return new ConsentController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public void Index_Get_ReturnsViewWithModel_WhenConsentCookieIsMissing()
        {
            var controller = CreateController();

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DataConsentViewModel>(viewResult.Model);
        }

        [Fact]
        public void Index_Get_RedirectsToApplyApplications_WhenUserConsentCookieExists()
        {
            var controller = CreateController(hasConsentCookie: true);

            var result = controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Applications", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Apply", redirectResult.RouteValues["area"]);
            });
        }

        [Fact]
        public void Index_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            var controller = CreateController();
            var model = new DataConsentViewModel();

            controller.ModelState.AddModelError(nameof(DataConsentViewModel.HasAccepted), "Required");

            var result = controller.Index(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
        }

        [Fact]
        public void Index_Post_WritesConsentCookieAndRedirectsToApplyApplications_WhenModelStateIsValid()
        {
            var controller = CreateController();
            var model = new DataConsentViewModel
            {
                HasAccepted = true
            };

            var result = controller.Index(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var setCookieHeader = controller.Response.Headers.SetCookie.ToString();

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Applications", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Apply", redirectResult.RouteValues["area"]);
                Assert.Contains(ConsentController.GetConsentCookieName(controller.HttpContext), setCookieHeader);
                Assert.Contains("true", setCookieHeader);
            });
        }

        [Fact]
        public void GetConsentCookieName_ReturnsSameCookieName_WhenEmailCaseAndWhitespaceDiffer()
        {
            var firstContext = CreateHttpContextWithClaims(new Claim("email", " AO.User@Test.com "));
            var secondContext = CreateHttpContextWithClaims(new Claim("email", "ao.user@test.com"));

            var firstCookieName = ConsentController.GetConsentCookieName(firstContext);
            var secondCookieName = ConsentController.GetConsentCookieName(secondContext);

            Assert.Multiple(() =>
            {
                Assert.NotEqual(ConsentController.ConsentCookieName, firstCookieName);
                Assert.Equal(firstCookieName, secondCookieName);
            });
        }

        [Fact]
        public void GetConsentCookieName_ReturnsDifferentCookieNames_ForDifferentUsers()
        {
            var firstContext = CreateHttpContextWithClaims(new Claim("email", "first.user@test.com"));
            var secondContext = CreateHttpContextWithClaims(new Claim("email", "second.user@test.com"));

            var firstCookieName = ConsentController.GetConsentCookieName(firstContext);
            var secondCookieName = ConsentController.GetConsentCookieName(secondContext);

            Assert.NotEqual(firstCookieName, secondCookieName);
        }

        [Fact]
        public void GetConsentCookieName_UsesClaimTypesEmail_WhenEmailClaimIsMissing()
        {
            var httpContext = CreateHttpContextWithClaims(new Claim(ClaimTypes.Email, "standard.email@test.com"));

            var cookieName = ConsentController.GetConsentCookieName(httpContext);

            Assert.Multiple(() =>
            {
                Assert.StartsWith($"{ConsentController.ConsentCookieName}_", cookieName);
                Assert.NotEqual(ConsentController.ConsentCookieName, cookieName);
            });
        }

        [Fact]
        public void GetConsentCookieName_UsesNameIdentifier_WhenEmailClaimsAreMissing()
        {
            var httpContext = CreateHttpContextWithClaims(new Claim(ClaimTypes.NameIdentifier, "user-identifier"));

            var cookieName = ConsentController.GetConsentCookieName(httpContext);

            Assert.Multiple(() =>
            {
                Assert.StartsWith($"{ConsentController.ConsentCookieName}_", cookieName);
                Assert.NotEqual(ConsentController.ConsentCookieName, cookieName);
            });
        }

        [Fact]
        public void GetConsentCookieName_UsesIdentityName_WhenIdentifierClaimsAreMissing()
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "identity.name@test.com")],
                    "test",
                    ClaimTypes.Name,
                    ClaimTypes.Role))
            };

            var cookieName = ConsentController.GetConsentCookieName(httpContext);

            Assert.Multiple(() =>
            {
                Assert.StartsWith($"{ConsentController.ConsentCookieName}_", cookieName);
                Assert.NotEqual(ConsentController.ConsentCookieName, cookieName);
            });
        }

        [Fact]
        public void GetConsentCookieName_ReturnsBaseCookieName_WhenUserIdentifierIsMissing()
        {
            var httpContext = new DefaultHttpContext();

            var cookieName = ConsentController.GetConsentCookieName(httpContext);

            Assert.Equal(ConsentController.ConsentCookieName, cookieName);
        }

        private static HttpContext CreateHttpContextWithClaims(params Claim[] claims)
        {
            return new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            };
        }
    }
}
