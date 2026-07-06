using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Consent;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class ConsentControllerTests
    {
        private static ConsentController CreateController(bool hasConsentCookie = false)
        {
            var httpContext = new DefaultHttpContext();

            if (hasConsentCookie)
            {
                httpContext.Request.Headers.Cookie = $"{ConsentController.ConsentCookieName}=true";
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
        public void Index_Get_RedirectsToReviewHome_WhenConsentCookieExists()
        {
            var controller = CreateController(hasConsentCookie: true);

            var result = controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Home", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Review", redirectResult.RouteValues["area"]);
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
                Assert.Contains(ConsentController.ConsentCookieName, setCookieHeader);
                Assert.Contains("true", setCookieHeader);
            });
        }
    }
}
