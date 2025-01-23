using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models.Routing;

namespace SFA.DAS.AODP.Web.Controllers;

public class RouteController : Controller
{
    [HttpGet()]
    [Route("form/{formVersionId}/routing/add-route")]
    public async Task<IActionResult> Route(Guid formVersionId)
    {
        return View(new CreateRouteViewModel
        {
            FormVersionId = formVersionId,
        });
    }
}
