using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Models.Routing;

namespace SFA.DAS.AODP.Web.Controllers;

public class RoutesController : Controller
{
    private readonly IMediator _mediator;

    public RoutesController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
        var query = new GetAllSectionsQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (!response.Success) return NotFound();

        return View(CreateRouteChooseSectionViewModel.MapToViewModel(response.Value, formVersionId));
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
