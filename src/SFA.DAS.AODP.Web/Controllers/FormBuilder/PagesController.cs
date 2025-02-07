using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class PagesController : Controller
{
    private readonly IMediator _mediator;

    public PagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/create")]
    public async Task<IActionResult> Create(Guid formVersionId, Guid sectionId)
    {
        return View(new CreatePageViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId
        });
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/create")]
    public async Task<IActionResult> Create(CreatePageViewModel model)
    {
        var command = new CreatePageCommand()
        {
            SectionId = model.SectionId,
            FormVersionId = model.FormVersionId,
            Description = model.Description,
            Title = model.Title

        };

        var response = await _mediator.Send(command);
        if (response.Success)
        {
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = response.Value.Id });
        }
        return View(model);
    }
    #endregion

    #region Edit
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}")]
    public async Task<IActionResult> Edit(Guid pageId, Guid sectionId, Guid formVersionId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId, formVersionId);
        var response = await _mediator.Send(query);
        if (response.Value == null) return NotFound();

        return View(EditPageViewModel.Map(response.Value, formVersionId));
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}")]
    public async Task<IActionResult> Edit(EditPageViewModel model)
    {
        if (model.AdditionalFormActions.MoveUp != default)
        {
            var command = new MoveQuestionUpCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                PageId = model.PageId,
                QuestionId = model.AdditionalFormActions.MoveDown ?? Guid.Empty
            };
            var response = await _mediator.Send(command);
            return View(model);
        }
        else if (model.AdditionalFormActions.MoveDown != default)
        {
            var command = new MoveQuestionUpCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                PageId = model.PageId,
                QuestionId = model.AdditionalFormActions.MoveDown ?? Guid.Empty
            };
            var response = await _mediator.Send(command);
            return View(model);
        }
        else
        {
            var command = new UpdatePageCommand()
            {
                Description = model.Description,
                Title = model.Title,
                SectionId = model.SectionId,
                FormVersionId = model.FormVersionId,
                Id = model.PageId
            };

            var response = await _mediator.Send(command);
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
        }
    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId, formVersionId);
        var response = await _mediator.Send(query);
        if (response.Value == null) return NotFound();
        return View(new DeletePageViewModel()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            Title = response.Value.Title
        });
    }

    [HttpPost]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeletePageViewModel model)
    {
        var command = new DeletePageCommand()
        {
            PageId = model.PageId,
            SectionId = model.SectionId,
            FormVersionId = model.FormVersionId
        };

        var deleteResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Sections", new { formVersionId = model.FormVersionId, sectionId = model.SectionId });

    }
    #endregion
}
