using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class PagesController : Controller
{
    private readonly IMediator _mediator;

    public PagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    [Route("pages/create/sections/{sectionId}/forms/{formVersionId}")]
    public async Task<IActionResult> Create(Guid sectionId, Guid formVersionId)
    {
        var query = new GetAllPagesQuery(sectionId);
        var response = await _mediator.Send(query);
        ViewData["Pages"] = response.Data;
        return View(new Page { SectionId = sectionId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePageCommand.Page page)
    {
        var command = new CreatePageCommand(page);

        var response = await _mediator.Send(command);
        return RedirectToAction("Edit", "Section", new { id = response.Data.SectionId });
    }
    #endregion

    #region Edit
    [Route("pages/edit/{pageId}/sections/{sectionId}/forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid pageId, Guid sectionId, Guid formVersionId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();

        var pagesQuery = new GetAllPagesQuery(response.Data.SectionId);
        var pagesResponse = await _mediator.Send(pagesQuery);
        ViewData["Pages"] = pagesResponse.Data;

        return View(response.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdatePageCommand.Page page)
    {
        var command = new UpdatePageCommand(page.Id, page);

        var response = await _mediator.Send(command);
        return RedirectToAction("Edit", "Section", new { id = response.Data.SectionId });
    }
    #endregion

    #region Delete
    [Route("pages/delete/{pageId}/sections/{sectionId}/forms/{formVersionId}")]
    public async Task<IActionResult> Delete(Guid pageId, Guid sectionId, Guid formVersionId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(response.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid pageId, Guid sectionId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId);
        var response = await _mediator.Send(query);
        if (response.Data != null)
        {
            var command = new DeletePageCommand(response.Data.Id);
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Section", new { id = response.Data.SectionId });
        }
        return NotFound();
    }
    #endregion
}
