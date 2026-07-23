using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Consent;
using SFA.DAS.AODP.Web.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Authorize(Policy = PolicyConstants.IsApplyUser)]
    public class ConsentController : Controller
    {
        public const string ConsentCookieName = "AODP_QFAST_PRIVACY_ACCEPTED";

        [HttpGet]
        public IActionResult Index()
        {
            if (Request.Cookies.ContainsKey(GetConsentCookieName(HttpContext)))
            {
                return RedirectToAction("Index", "Applications", new { area = "Apply" });
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

            Response.Cookies.Append(GetConsentCookieName(HttpContext), "true", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            return RedirectToAction("Index", "Applications", new { area = "Apply" });
        }
        public static string GetConsentCookieName(HttpContext httpContext)
        {
            var userIdentifier =
                httpContext.User.FindFirst("email")?.Value ??
                httpContext.User.FindFirst(ClaimTypes.Email)?.Value ??
                httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                httpContext.User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return ConsentCookieName;
            }

            var userHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(userIdentifier.Trim().ToLowerInvariant())));

            return $"{ConsentCookieName}_{userHash[..16]}";
        }
    }
}