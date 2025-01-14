using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class SectionController : Controller
{
    public IMediator _mediator { get; }

    public SectionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create
    public async Task<IActionResult> Create(Guid formId)
    {
        var sectionsQuery = await _mediator.Send(new GetAllSectionsQuery { FormId = formId });
        ViewData["Sections"] = sectionsQuery.Data;
        return View(new Section { FormId = formId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Section section)
    {
        var command = new CreateSectionCommand
        {
            FormId = section.FormId,
            Order = section.Order,
            Title = section.Title,
            Description = section.Description,
            NextSectionId = section.NextSectionId,
        };

        var createSectionResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Form", new { id = createSectionResponse.Data!.FormId });
    }
    #endregion

    #region Edit
    public async Task<IActionResult> Edit(Guid id)
    {
        var sectionQuery = await _mediator.Send(new GetSectionByIdQuery { Id = id });
        if (sectionQuery.Data == null) return NotFound();

        var sectionsQuery = await _mediator.Send(new GetAllSectionsQuery { FormId = sectionQuery.Data.FormId });
        var pagesQuery = await _mediator.Send(new GetAllPagesQuery { SectionId = sectionQuery.Data.Id });
        ViewData["Sections"] = sectionsQuery.Data;
        ViewData["Pages"] = pagesQuery.Data;

        return View(sectionQuery.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Section section)
    {
        var command = new UpdateSectionCommand
        {
            Id = section.Id,
            FormId = section.FormId,
            Order = section.Order,
            Title = section.Title,
            Description = section.Description,
            NextSectionId = section.NextSectionId
        };

        var updateSectionResponse = await _mediator.Send(command);
        return RedirectToAction("Edit", "Form", new { id = section.FormId });
    }
    #endregion

    #region Delete
    public async Task<IActionResult> Delete(Guid id)
    {
        var sectionQuery = await _mediator.Send(new GetSectionByIdQuery { Id = id });
        if (sectionQuery.Data == null) return NotFound();
        return View(sectionQuery.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var sectionQuery = await _mediator.Send(new GetSectionByIdQuery { Id = id });
        if (sectionQuery.Data != null)
        {
            var command = new DeleteSectionCommand { Id = sectionQuery.Data.Id };
            var deleteSectionResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Form", new { id = sectionQuery.Data.FormId });
        }
        return NotFound();
    }
    #endregion
}