using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Models.FormBuilder.Section;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

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
            Title = model.Title
        };

        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = response.Value.Id });
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
        if (response.Value == null) return NotFound();

        return View(EditSectionViewModel.Map(response.Value));
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(EditSectionViewModel model)
    {
        if (model.AdditionalActions.MoveUp != default)
        {
            var command = new MovePageUpCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                PageId = model.AdditionalActions.MoveUp ?? Guid.Empty,
            };
            var response = await _mediator.Send(command);
            return await Edit(model.FormVersionId, model.SectionId);
        }
        else if (model.AdditionalActions.MoveDown != default)
        {
            var command = new MovePageDownCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                PageId = model.AdditionalActions.MoveDown ?? Guid.Empty,
            };
            var response = await _mediator.Send(command);
            return await Edit(model.FormVersionId, model.SectionId);
        }
        else
        {
            var command = new UpdateSectionCommand()
            {
                FormVersionId = model.FormVersionId,
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

    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> Delete(Guid sectionId, Guid formVerisonId)
    {
        var query = new GetSectionByIdQuery(sectionId, formVerisonId);
        var response = await _mediator.Send(query);
        if (response.Value == null) return NotFound();
        return View(new DeleteSectionViewModel()
        {
            Title = response.Value.Title,
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