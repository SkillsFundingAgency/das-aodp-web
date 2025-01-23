using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models.Routing;

namespace SFA.DAS.AODP.Web.Controllers;

public class RouteController : Controller
{
    [HttpGet()]
    [Route("form/{formVersionId}/routing/add-route")]
    public async Task<IActionResult> Create(Guid formVersionId)
    {
        return View(new CreateRouteViewModel
        {
            FormVersionId = formVersionId,
        });
    }

    [HttpGet()]
    [Route("form/{formVersionId}/routing/choose-section")]
    public async Task<IActionResult> ChooseSection(Guid formVersionId)
    {
        return View(new CreateRouteChooseSectionViewModel
        {
            FormVersionId = formVersionId,
            Sections = new()
            {
                new()
                {
                     Key = Guid.NewGuid(),
                     Order = 1,
                     Title = "Section 1"
                }
            }
        });
    }

    [HttpGet()]
    [Route("form/{formVersionId}/routing/section/{sectionKey}/choose-page")]
    public async Task<IActionResult> ChoosePage(Guid formVersionId, Guid sectionKey)
    {
        return View(new CreateRouteChoosePageViewModel
        {
            FormVersionId = formVersionId,
            Pages = new() { new()
            {
                 Key = Guid.NewGuid(),
                 Order = 1,
                 Title = "Page 1"
            }
            }
        });
    }

    [HttpGet()]
    [Route("form/{formVersionId}/routing/section/{sectionKey}/page/{pageKey}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(Guid formVersionId, Guid sectionKey, Guid pageKey)
    {
        return View(new CreateRouteChooseQuestionViewModel
        {
            FormVersionId = formVersionId,
            Questions = new()
             {
                 new()
                 {
                      Key = Guid.NewGuid(),
                     Order = 1,
                     Title = "Question 1"
                 }
             }
        });
    }

    [HttpGet()]
    [Route("form/{formVersionId}/routes")]
    public async Task<IActionResult> List(Guid formVersionId)
    {
        return View(new ListRoutesViewModel
        {
            FormVersionId = formVersionId,
        });
    }
}
