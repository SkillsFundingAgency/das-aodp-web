using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.AODP.Web.Extensions;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

public class RolloverSessionCheckActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session.GetObject<Rollover>("RolloverSession");
        if (session is null && ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ActionName != "Index")
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }

        base.OnActionExecuting(context);
    }
}