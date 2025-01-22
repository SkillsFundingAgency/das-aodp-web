using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

namespace SFA.DAS.AODP.Web.Controllers;

public class SectionsController : Controller
{
    public IMediator _mediator { get; }

    public SectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    [Route("sections/create/forms/{formVersionId}")]
    public async Task<IActionResult> Create(Guid formVersionId)
    {
        var query = new GetAllSectionsQuery(formVersionId);
        var response = await _mediator.Send(query);
        ViewData["Sections"] = response.Data;
        return View(new GetAllSectionsQueryResponse.Section { FormVersionId = formVersionId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSectionCommand.Section section)
    {
        var command = new CreateSectionCommand(section);

        var response = await _mediator.Send(command);
        return RedirectToAction("Edit", "Form", new { id = response.Data.FormVersionId });
    }
    #endregion

    #region Edit
    [Route("sections/edit/{sectionId}/forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid sectionId, Guid formVersionId)
    {
        var sectionQuery = new GetSectionByIdQuery(sectionId, formVersionId);
        var response = await _mediator.Send(sectionQuery);
        if (response.Data == null) return NotFound();

        var sectionsQuery = new GetAllSectionsQuery(response.Data.FormVersionId);
        var sectionsResponse = await _mediator.Send(sectionsQuery);
        var pagesQuery = new GetAllPagesQuery(response.Data.Id);
        var pagesResponse = await _mediator.Send(pagesQuery);
        ViewData["Sections"] = sectionsResponse.Data;
        ViewData["Pages"] = pagesResponse.Data;

        return View(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateSectionCommand.Section section)
    {
        var command = new UpdateSectionCommand(section.FormVersionId, section);

        var response = await _mediator.Send(command);
        return RedirectToAction("Edit", "Form", new { id = response.Data.FormVersionId });
    }
    #endregion

    #region Delete
    [Route("sections/delete/{sectionId}/forms/{formVersionId}")]
    public async Task<IActionResult> Delete(Guid sectionId, Guid formVerisonId)
    {
        var query = new GetSectionByIdQuery(sectionId, formVerisonId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(response.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid sectionId, Guid formVerisonId)
    {
        var query = new GetSectionByIdQuery(sectionId, formVerisonId);
        var response = await _mediator.Send(query);
        if (response.Data != null)
        {
            var command = new DeleteSectionCommand(response.Data.Id);
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Form", new { id = response.Data.FormVersionId });
        }
        return NotFound();
    }
    #endregion
}