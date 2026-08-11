using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Filters;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Filters
{
    public class RequireDataConsentFilterTests
    {
        private readonly RequireDataConsentFilter _filter = new();

        private static ActionExecutingContext CreateActionExecutingContext(
            string? area,
            string? controller,
            bool hasConsentCookie = false,
            bool isAoUser = false)
        {
            var httpContext = new DefaultHttpContext();

            if (isAoUser)
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("rolecode", "ao_user"),
                    new Claim("email", "ao.user@test.com")
                }, "test"));
            }

            if (hasConsentCookie)
            {
                httpContext.Request.Headers.Cookie = $"{ConsentController.GetConsentCookieName(httpContext)}=true";
            }

            var routeData = new RouteData();

            if (area is not null)
            {
                routeData.Values["area"] = area;
            }

            if (controller is not null)
            {
                routeData.Values["controller"] = controller;
            }

            var actionContext = new ActionContext(
                httpContext,
                routeData,
                new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                new object());
        }

        [Fact]
        public void OnActionExecuting_DoesNothing_WhenAreaIsNotReviewOrApply()
        {
            var context = CreateActionExecutingContext("Admin", "Import");

            _filter.OnActionExecuting(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void OnActionExecuting_DoesNothing_WhenControllerIsConsent()
        {
            var context = CreateActionExecutingContext("Review", "Consent", isAoUser: true);

            _filter.OnActionExecuting(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void OnActionExecuting_DoesNothing_WhenConsentCookieExists()
        {
            var context = CreateActionExecutingContext("Review", "Home", hasConsentCookie: true, isAoUser: true);

            _filter.OnActionExecuting(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void OnActionExecuting_DoesNothing_WhenReviewAreaAndUserIsNotAo()
        {
            var context = CreateActionExecutingContext("Review", "Home");

            _filter.OnActionExecuting(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void OnActionExecuting_RedirectsToConsent_WhenReviewAreaAoUserAndConsentCookieIsMissing()
        {
            var context = CreateActionExecutingContext("Review", "Home", isAoUser: true);

            _filter.OnActionExecuting(context);

            var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Consent", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Review", redirectResult.RouteValues["area"]);
            });
        }

        [Fact]
        public void OnActionExecuting_RedirectsToConsent_WhenApplyAreaAoUserAndConsentCookieIsMissing()
        {
            var context = CreateActionExecutingContext("Apply", "Applications", isAoUser: true);

            _filter.OnActionExecuting(context);

            var redirectResult = Assert.IsType<RedirectToActionResult>(context.Result);

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Consent", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Review", redirectResult.RouteValues["area"]);
            });
        }
        [Fact]
        public void OnActionExecuted_DoesNotThrow()
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor());

            var context = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new object());

            _filter.OnActionExecuted(context);
        }
    }
}
