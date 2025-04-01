using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.Help;

namespace SFA.DAS.AODP.Web.Controllers
{
    [Authorize]
    public class HelpController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CookiesPolicy()
        {
            var analyticsCookieValue = Request.Cookies[CookieKeys.AnalyticsConsent];
            var functionalCookieValue = Request.Cookies[CookieKeys.FunctionalConsent];

            _ = bool.TryParse(analyticsCookieValue, out var isAnalyticsCookieConsentGiven);
            _ = bool.TryParse(functionalCookieValue, out var isFunctionalCookieConsentGiven);

            var referer = Request.Headers.Referer.FirstOrDefault();

            var cookieViewModel = new CookiesViewModel
            {
                PreviousPageUrl = referer ?? Url.RouteUrl("default") ?? "/",
                ConsentAnalyticsCookie = isAnalyticsCookieConsentGiven,
                ConsentFunctionalCookie = isFunctionalCookieConsentGiven
            };
            return View(cookieViewModel);
        }
    }
}
