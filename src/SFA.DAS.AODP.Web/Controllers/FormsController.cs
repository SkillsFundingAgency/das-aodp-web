using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

namespace SFA.DAS.AODP.Web.Controllers;

public class FormsController : Controller
{
    private readonly IMediator _mediator;

    public FormsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Main
    public async Task<IActionResult> Index()
    {
        var query = new GetAllFormVersionsQuery();
        var response = await _mediator.Send(query);
        if (response.Data!.Count == 0) return RedirectToAction(nameof(Create));
        return View(response.Data);
    }
    #endregion

    #region Create
    [Route("forms/create")]
    public IActionResult Create()
    {
        return View(new CreateFormVersionCommand.FormVersion());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFormVersionCommand.FormVersion formVersion)
    {
        var command = new CreateFormVersionCommand(formVersion);

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    [Route("forms/edit/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(formVersionQuery);
        if (response.Data == null) return NotFound();

        var sectionsQuery = new GetAllSectionsQuery(response.Data.Id);
        var sectionsResponse = await _mediator.Send(sectionsQuery);
        ViewData["Sections"] = sectionsResponse.Data;
        return View(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateFormVersionCommand.FormVersion formVersion)
    {
        var command = new UpdateFormVersionCommand(formVersion.Id, formVersion);

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    [Route("forms/delete/{formVersionId}")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(response.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Data != null)
        {
            var command = new DeleteFormVersionCommand(response.Data.Id);
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
        return NotFound();

    }
    #endregion
}