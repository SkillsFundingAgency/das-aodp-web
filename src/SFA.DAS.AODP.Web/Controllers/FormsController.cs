using MediatR;
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
    public async Task<IActionResult> Create(CreateFormVersionViewModel viewModel)
    {
        #region Command Model Binding (CreateFormVersion -> CreateFormVersionCommand.FormVersion)
        var commandModel = new CreateFormVersionCommand.FormVersion();
        commandModel.Id = Guid.NewGuid();
        commandModel.Name = viewModel.Name;
        commandModel.Description = viewModel.Description;
        commandModel.Version = viewModel.Version;
        //commandModel.Status = viewModel.SelectedStatus;
        commandModel.Order = viewModel.Order;
        commandModel.DateCreated = viewModel.DateCreated;
        #endregion

        var command = new CreateFormVersionCommand(commandModel);

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Edit
    [Route("forms/edit/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(formVersionQuery);
        if (response.Data == null) return NotFound();

        var viewModel = EditFormVersionViewModel.Map(response);


        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateFormVersionCommand.FormVersion formVersion)
    {
        var command = new UpdateFormVersionCommand(formVersion.Id, formVersion);

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Delete
    [Route("forms/delete/{formVersionId}")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Data == null) return NotFound();
        return View(response.Data);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Data != null)
        {
            var command = new DeleteFormVersionCommand(response.Data.Id);
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
        return NotFound();

    }
    #endregion
}