using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Models.Forms.FormBuilder;
using SFA.DAS.AODP.Web.Models.Question;

namespace SFA.DAS.AODP.Web.Controllers;

public class QuestionController : Controller
{
    private readonly IMediator _mediator;

    public QuestionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create

    [HttpGet()]
    [Route("form/{formVersionId}/section/{sectionKey}/page/{pageKey}/add-question")]
    public async Task<IActionResult> Create(Guid formVersionId, Guid sectionKey, Guid pageKey)
    {
        return View(new CreateQuestionViewModel
        {
            PageKey = pageKey,
            FormVersionId = formVersionId,
            SectionKey = sectionKey
        });
    }

    [HttpPost()]
    [Route("form/{formVersionId}/section/{sectionKey}/page/{pageKey}/add-question")]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        return RedirectToAction(nameof(Edit), new
        {
            formVersionId = model.FormVersionId,
            sectionKey = model.SectionKey,
            pageKey = model.PageKey,
            questionId = Guid.NewGuid()
        });
    }


    [HttpGet()]
    [Route("form/{formVersionId}/section/{sectionKey}/page/{pageKey}/question/{questionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionKey, Guid pageKey, Guid questionId)
    {
        return View(new EditQuestionViewModel
        {
            PageKey = pageKey,
            FormVersionId = formVersionId,
            SectionKey = sectionKey,
            Id = questionId
        });
    }


    //[HttpPost]
    //public async Task<IActionResult> Create(Page page)
    //{
    //    var command = new CreatePageCommand
    //    {
    //        SectionId = page.SectionId,
    //        Title = page.Title,
    //        Description = page.Description,
    //        Order = page.Order,
    //        NextPageId = page.NextPageId,
    //    };

    //    var createPageResponse = await _mediator.Send(command);
    //    return RedirectToAction("Edit", "Section", new { id = page.SectionId });
    //}
    #endregion

    #region Edit
    public async Task<IActionResult> Edit(Guid id)
    {
        var pageQuery = await _mediator.Send(new GetPageByIdQuery { Id = id });
        if (pageQuery.Data == null) return NotFound();

        var pagesQuery = await _mediator.Send(new GetAllPagesQuery { SectionId = pageQuery.Data.SectionId });
        ViewData["Pages"] = pagesQuery.Data;

        return View(pageQuery.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Page page)
    {
        var command = new UpdatePageCommand
        {
            Id = page.Id,
            SectionId = page.SectionId,
            Title = page.Title,
            Description = page.Description,
            Order = page.Order,
            NextPageId = page.NextPageId,
        };

        var updatePageResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Section", new { id = page.SectionId });
    }
    #endregion

    #region Delete
    public async Task<IActionResult> Delete(Guid id)
    {
        var pageQuery = await _mediator.Send(new GetPageByIdQuery { Id = id });
        if (pageQuery.Data == null) return NotFound();
        return View(pageQuery.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var pageQuery = await _mediator.Send(new GetPageByIdQuery { Id = id });
        if (pageQuery.Data != null)
        {
            var command = new DeletePageCommand { Id = pageQuery.Data.Id };
            var deletePageResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Section", new { id = pageQuery.Data.SectionId });
        }
        return NotFound();
    }
    #endregion
}
