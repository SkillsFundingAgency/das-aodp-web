using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class PageController : Controller
{
    private readonly IMediator _mediator;

    public PageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    public async Task<IActionResult> Create(Guid sectionId)
    {
        var pagesQuery = await _mediator.Send(new GetAllPagesQuery { SectionId = sectionId });
        ViewData["Pages"] = pagesQuery.Data;
        return View(new Page { SectionId = sectionId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Page page)
    {
        var command = new CreatePageCommand
        {
            SectionId = page.SectionId,
            Title = page.Title,
            Description = page.Description,
            Order = page.Order,
            NextPageId = page.NextPageId,
        };

        var createPageResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Section", new { id = createPageResponse.Data!.SectionId });
    }
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
