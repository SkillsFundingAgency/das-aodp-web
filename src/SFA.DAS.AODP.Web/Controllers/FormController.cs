using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers;

public class FormController : Controller
{
    private readonly IMediator _mediator;

    public FormController(IMediator mediator) {
        _mediator = mediator;
    }

    #region Main
    public async Task<IActionResult> Index()
    {
        var formsQuery = await _mediator.Send(new GetAllFormsQuery());
        if (formsQuery.Data!.Count == 0) return RedirectToAction(nameof(Create));
        return View(formsQuery.Data);
    }
    #endregion

    #region Create
    public IActionResult Create()
    {
        return View(new Form());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Form form)
    {
        var command = new CreateFormCommand
        {
            Name = form.Name,
            Version = form.Version,
            Published = form.Published,
            Key = form.Key,
            ApplicationTrackingTemplate = form.ApplicationTrackingTemplate,
            Order = form.Order,
            Description = form.Description,
        };

        var createFormResponse = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    public async Task<IActionResult> Edit(Guid id)
    {
        var formQuery = await _mediator.Send(new GetFormByIdQuery { Id = id });
        if (formQuery.Data == null) return NotFound();

        var sectionQuery = await _mediator.Send(new GetAllSectionsQuery { FormId = formQuery.Data.Id });
        ViewData["Sections"] = sectionQuery.Data;
        return View(formQuery.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Form form)
    {
        var command = new UpdateFormCommand
        {
            Id = form.Id,
            Name = form.Name,
            Version = form.Version,
            Published = form.Published,
            Key = form.Key,
            ApplicationTrackingTemplate = form.ApplicationTrackingTemplate,
            Order = form.Order,
            Description = form.Description,
        };

        var updateFormResponse = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    public async Task<IActionResult> Delete(Guid id)
    {
        var formQuery = await _mediator.Send(new GetFormByIdQuery { Id = id });
        if (formQuery.Data == null) return NotFound();
        return View(formQuery.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var formQuery = await _mediator.Send(new GetFormByIdQuery { Id = id });
        if (formQuery.Data != null)
        {
            var command = new DeleteFormCommand { Id = formQuery.Data.Id };
            var deleteFormResponse = await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
        return NotFound();

    }
    #endregion
}