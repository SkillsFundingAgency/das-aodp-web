using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using System.Reflection;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

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
        if(!response.Success)
        {
            return Redirect("/Home/Error");
        }

        var viewModel = FormVersionListViewModel.Map(response.Value);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(FormVersionListViewModel model)
    {
        if (model.AdditionalActions.CreateDraft.HasValue)
        {
            var command = new CreateDraftFormVersionCommand(model.AdditionalActions.CreateDraft.Value);
            var response = await _mediator.Send(command);
            if (response.Success)
            {
                return RedirectToAction(nameof(Edit), new { formVersionId = response.Value.FormVersionId });
            }
            ViewBag.InternalServerError = true;
            return View(model);
        }
        else if (model.AdditionalActions.MoveDown.HasValue)
        {
            var command = new MoveFormDownCommand(model.AdditionalActions.MoveDown.Value);
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(model);
            }
        }
        else if (model.AdditionalActions.MoveUp.HasValue)
        {
            var command = new MoveFormUpCommand(model.AdditionalActions.MoveUp.Value);
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(model);
            }
        }

        return RedirectToAction(nameof(Index));
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
        };

        var response = await _mediator.Send(command);
        if (!response.Success)
        {
            ViewBag.InternalServerError = true;
            return View(viewModel);
        }
        return RedirectToAction(nameof(Edit), new { formVersionId = response.Value.Id });
    }
    #endregion

    #region Edit
    [HttpGet]
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(formVersionQuery);
        if (!response.Success) return Redirect("Error");

        var viewModel = EditFormVersionViewModel.Map(response.Value);


        return View(viewModel);
    }

    [HttpPost]
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(EditFormVersionViewModel editFormVersionViewModel)
    {
        if (editFormVersionViewModel.AdditionalFormActions.Publish != default)
        {
            var command = new PublishFormVersionCommand(editFormVersionViewModel.Id);
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(editFormVersionViewModel);
            }
        }
        else if (editFormVersionViewModel.AdditionalFormActions.UnPublish != default)
        {
            var command = new UnpublishFormVersionCommand(editFormVersionViewModel.Id);
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(editFormVersionViewModel);
            }
        }
        else if (editFormVersionViewModel.AdditionalFormActions.MoveUp != default)
        {
            var command = new MoveSectionUpCommand()
            {
                FormVersionId = editFormVersionViewModel.Id,
                SectionId = editFormVersionViewModel.AdditionalFormActions.MoveUp ?? Guid.Empty,
            };
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(editFormVersionViewModel);
            }
        }
        else if (editFormVersionViewModel.AdditionalFormActions.MoveDown != default)
        {
            var command = new MoveSectionDownCommand()
            {
                FormVersionId = editFormVersionViewModel.Id,
                SectionId = editFormVersionViewModel.AdditionalFormActions.MoveDown ?? Guid.Empty,
            };
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(editFormVersionViewModel);
            }
        }
        else
        {
            var command = new UpdateFormVersionCommand()
            {
                FormVersionId = editFormVersionViewModel.Id,
                Description = editFormVersionViewModel.Description,
                Order = editFormVersionViewModel.Order,
                Name = editFormVersionViewModel.Title
            };
            var response = await _mediator.Send(command);
            if (!response.Success)
            {
                ViewBag.InternalServerError = true;
                return View(editFormVersionViewModel);
            }
        }
        return RedirectToAction(nameof(Edit), new { formVersionId = editFormVersionViewModel.Id });
    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await _mediator.Send(query);
        if (response.Value == null || !response.Success) return Redirect("/Home/Error");
        return View(new DeleteFormViewModel()
        {
            FormVersionId = formVersionId,
            Title = response.Value.Title
        });
    }

    [HttpPost]
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteFormViewModel model)
    {
        var command = new DeleteFormVersionCommand(model.FormVersionId);
        var deleteResponse = await _mediator.Send(command);
        if (!deleteResponse.Success)
        {
            ViewBag.InternalServerError = true;
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }
    #endregion
}