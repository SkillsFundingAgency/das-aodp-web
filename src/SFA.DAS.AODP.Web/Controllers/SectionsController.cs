using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Models.Section;

namespace SFA.DAS.AODP.Web.Controllers;

public class SectionsController : Controller
{
    public IMediator _mediator { get; }

    public SectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    [Route("forms/{formVersionId}/sections/create")]
    public async Task<IActionResult> Create(Guid formVersionId)
    {
        return View(new CreateSectionViewModel { FormVersionId = formVersionId });
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/create")]
    public async Task<IActionResult> Create(CreateSectionViewModel model)
    {
        var command = new CreateSectionCommand()
        {
            FormVersionId = model.FormVersionId,
            Description = model.Description,
            Title = model.Title
        };

        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = response.Id });
        }
        return View(model);
    }
    #endregion

    #region Edit
    [Route("forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId)
    {
        var sectionQuery = new GetSectionByIdQuery(sectionId, formVersionId);
        var response = await _mediator.Send(sectionQuery);
        if (response.Data == null) return NotFound();

        return View(EditSectionViewModel.Map(response));
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(EditSectionViewModel model)
    {
        var command = new UpdateSectionCommand()
        {
            FormVersionId = model.FormVersionId,
            Description = model.Description,
            Title = model.Title,
            Id = model.SectionId
        };

        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId });
        }

        return View(model);

    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> Delete(Guid sectionId, Guid formVerisonId)
    {
        var query = new GetSectionByIdQuery(sectionId, formVerisonId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(new DeleteSectionViewModel()
        {
            Title = response.Data.Title,
            SectionId = sectionId,
            FormVersionId = formVerisonId
        });
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteSectionViewModel model)
    {
        var command = new DeleteSectionCommand()
        {
            FormVersionId = model.FormVersionId,
            SectionId = model.SectionId
        };
        var deleteResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Form", new { id = model.FormVersionId });

    }
    #endregion
}