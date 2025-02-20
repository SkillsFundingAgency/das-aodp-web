using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class FormsController : ControllerBase
{
    private const string FormUpdatedKey  = nameof(FormUpdatedKey);

    public FormsController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
    {    }

    #region Main
    public async Task<IActionResult> Index()
    {
        try
        {
            var query = new GetAllFormVersionsQuery();
            var response = await Send(query);

            var viewModel = FormVersionListViewModel.Map(response);

            return View(viewModel);
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Index(FormVersionListViewModel model)
    {
        try
        {
            if (model.AdditionalActions.CreateDraft.HasValue)
            {
                var command = new CreateDraftFormVersionCommand(model.AdditionalActions.CreateDraft.Value);
                var response = await Send(command);
                return RedirectToAction(nameof(Edit), new { formVersionId = response.FormVersionId });

            }
            else if (model.AdditionalActions.MoveDown.HasValue)
            {
                var command = new MoveFormDownCommand(model.AdditionalActions.MoveDown.Value);
                await Send(command);
            }
            else if (model.AdditionalActions.MoveUp.HasValue)
            {
                var command = new MoveFormUpCommand(model.AdditionalActions.MoveUp.Value);
                await Send(command);
            }

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View(model);
        }
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
        try
        {
            var command = new CreateFormVersionCommand
            {
                Title = viewModel.Name,
                Description = viewModel.Description,
            };

            var response = await Send(command);
            return RedirectToAction(nameof(Edit), new { formVersionId = response.Id });
        }
        catch
        {
            return View(viewModel);
        }
    }
    #endregion

    #region Edit
    [HttpGet]
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        try
        {
            var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
            var response = await Send(formVersionQuery);

            var viewModel = EditFormVersionViewModel.Map(response);

            ShowNotificationIfKeyExists(FormUpdatedKey, ViewNotificationMessageType.Success, "The form has been updated.");

            return View(viewModel);
        }
        catch
        {
            return Redirect("Error");
        }

    }

    [HttpPost]
    [Route("forms/{formVersionId}")]
    public async Task<IActionResult> Edit(EditFormVersionViewModel editFormVersionViewModel)
    {
        try
        {
            if (editFormVersionViewModel.AdditionalFormActions.Publish != default)
            {
                var command = new PublishFormVersionCommand(editFormVersionViewModel.Id);
                await Send(command);
            }
            else if (editFormVersionViewModel.AdditionalFormActions.UnPublish != default)
            {
                var command = new UnpublishFormVersionCommand(editFormVersionViewModel.Id);
                await Send(command);
            }
            else if (editFormVersionViewModel.AdditionalFormActions.MoveUp != default)
            {
                var command = new MoveSectionUpCommand()
                {
                    FormVersionId = editFormVersionViewModel.Id,
                    SectionId = editFormVersionViewModel.AdditionalFormActions.MoveUp ?? Guid.Empty,
                };
                await Send(command);
            }
            else if (editFormVersionViewModel.AdditionalFormActions.MoveDown != default)
            {
                var command = new MoveSectionDownCommand()
                {
                    FormVersionId = editFormVersionViewModel.Id,
                    SectionId = editFormVersionViewModel.AdditionalFormActions.MoveDown ?? Guid.Empty,
                };
                await Send(command);
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
                await Send(command);

                TempData[FormUpdatedKey] = true;
            }
            return RedirectToAction(nameof(Edit), new { formVersionId = editFormVersionViewModel.Id });
        }
        catch
        {
            return View(editFormVersionViewModel);
        }
    }
    #endregion

    #region Delete
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        try
        {
            var query = new GetFormVersionByIdQuery(formVersionId);
            var response = await Send(query);
            return View(new DeleteFormViewModel()
            {
                FormVersionId = formVersionId,
                Title = response.Title
            });
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost]
    [Route("forms/{formVersionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteFormViewModel model)
    {
        try
        {
            var command = new DeleteFormVersionCommand(model.FormVersionId);
            await Send(command);
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View(model);
        }
    }
    #endregion
}