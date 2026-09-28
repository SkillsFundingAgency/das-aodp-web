using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.AODP.Web.Extensions;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

public class RolloverSessionCheckActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session.GetObject<Rollover>("RolloverSession");
        var actionName = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context.ActionDescriptor).ActionName;
        if (session is null &&  !ActionsToIgnore.Contains(actionName))
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }

        base.OnActionExecuting(context);
    }

    private string[] ActionsToIgnore => [nameof(RolloverController.Index), nameof(RolloverController.RolloverSubmitted)];
}