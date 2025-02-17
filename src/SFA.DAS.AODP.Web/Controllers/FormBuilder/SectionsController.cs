using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Models.FormBuilder.Section;
using Azure;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class SectionsController : ControllerBase
{
    public SectionsController(IMediator mediator) : base(mediator)
    {
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
        try
        {
            var command = new CreateSectionCommand()
            {
                FormVersionId = model.FormVersionId,
                Title = model.Title
            };

            var response = await Send(command);
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = response.Id });
        }
        catch
        {
            return View(model);
        }
    }
    #endregion

    #region Edit
    [Route("forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId)
    {
        try
        {
            var sectionQuery = new GetSectionByIdQuery(sectionId, formVersionId);
            var response = await Send(sectionQuery);

            return View(EditSectionViewModel.Map(response));
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(EditSectionViewModel model)
    {
        try
        {
            if (model.AdditionalActions.MoveUp != default)
            {
                var command = new MovePageUpCommand()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.AdditionalActions.MoveUp ?? Guid.Empty,
                };
                await Send(command);
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
                await _mediator.Send(command);
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
                return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId });
            }
        }
        catch
        {
            return View(model);
        }
    }
    #endregion

    #region Delete
    [HttpGet]
    [Route("forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> Delete(Guid sectionId, Guid formVersionId)
    {
        try
        {
            var query = new GetSectionByIdQuery(sectionId, formVersionId);
            var response = await Send(query);
            return View(new DeleteSectionViewModel()
            {
                Title = response.Title,
                SectionId = sectionId,
                FormVersionId = formVersionId
            });
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost, ActionName("Delete")]
    [Route("forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteSectionViewModel model)
    {
        try
        {
            var command = new DeleteSectionCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId
            };
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Forms", new { formVersionId = model.FormVersionId });
        }
        catch
        {
            return View(model);
        }
    }
    #endregion
}