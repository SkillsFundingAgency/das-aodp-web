using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;

namespace SFA.DAS.AODP.Web.Filters
{
    public class RequireDataConsentFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var routeValues = context.RouteData.Values;
            var area = routeValues["area"]?.ToString();
            var controller = routeValues["controller"]?.ToString();

            if (!string.Equals(area, "Review", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(controller, "Consent", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.HttpContext.Request.Cookies.ContainsKey(ConsentController.ConsentCookieName))
            {
                return;
            }

            context.Result = new RedirectToActionResult("Index", "Consent", new { area = "Review" });
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}