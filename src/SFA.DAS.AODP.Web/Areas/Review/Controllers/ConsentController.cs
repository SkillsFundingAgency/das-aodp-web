using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Consent;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Authorize]
    public class ConsentController : Controller
    {
        public const string ConsentCookieName = "AODP_QFAST_PRIVACY_ACCEPTED";

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.ContainsKey(ConsentCookieName))
            {
                return RedirectToAction("Index", "Home", new { area = "Review" });
            }

            return View(new DataConsentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(DataConsentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Response.Cookies.Append(ConsentCookieName, "true", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            return RedirectToAction("Index", "Applications", new { area = "Apply" });
        }
    }
}