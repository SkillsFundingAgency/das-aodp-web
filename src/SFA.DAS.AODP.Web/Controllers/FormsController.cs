﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Models.Forms;

namespace SFA.DAS.AODP.Web.Controllers;

public class FormsController : Controller
{
    private readonly IMediator _mediator;

    public FormsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Main
    public async Task<IActionResult> Index()
    {
        var query = new GetAllFormVersionsQuery();
        var response = await _mediator.Send(query);

        var viewModel = FormVersionListViewModel.Map(response);

        return View(viewModel);
    }
    #endregion

    #region Create
    [Route("forms/create")]
    public IActionResult Create()
    {
        var viewModel = new CreateFormVersionViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [Route("forms/create")]
    public async Task<IActionResult> Create(CreateFormVersionViewModel viewModel)
    {
        var command = new CreateFormVersionCommand
        {
            Title = viewModel.Name,
            Description = viewModel.Description,
            Order = viewModel.Order
        };

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(formVersionQuery);
        if (response.Data == null) return NotFound();

        var viewModel = EditFormVersionViewModel.Map(response);


        return View(viewModel);
    }

    [HttpPost]
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(EditFormVersionViewModel editFormVersionViewModel)
    {
        var command = new UpdateFormVersionCommand()
        {
            FormVersionId = editFormVersionViewModel.Id,
            Description = editFormVersionViewModel.Description,
            Order = editFormVersionViewModel.Order,
            Name = editFormVersionViewModel.Title
        };

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Edit), new { formVersionId = editFormVersionViewModel.Id });
    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(new DeleteFormViewModel()
        {
            FormVersionId = formVersionId,
            Title = response.Data.Title
        });
    }

    [HttpPost]
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteFormViewModel model)
    {
        var command = new DeleteFormVersionCommand(model.FormVersionId);
        var deleteResponse = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion
}