﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Common.Exceptions;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.Markdown;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using static SFA.DAS.AODP.Web.Helpers.ListHelper.OrderButtonHelper;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
[Area("Admin")]
public class FormsController : ControllerBase
{
    public enum UpdateKeys { FormUpdated, FormPublished, FormDeleted }

    public FormsController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
    { }

    #region Main
    [Route("/admin/forms")]
    public async Task<IActionResult> Index()
    {
        var query = new GetAllFormVersionsQuery();
        var response = await Send(query);
        ShowNotificationIfKeyExists(UpdateKeys.FormDeleted.ToString(), ViewNotificationMessageType.Success, "The form has been deleted.");

        var viewModel = FormVersionListViewModel.Map(response);

        return View(viewModel);
    }

    [Route("/admin/forms")]
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
                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.FormId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();
                await Send(command);
            }
            else if (model.AdditionalActions.MoveUp.HasValue)
            {
                var command = new MoveFormUpCommand(model.AdditionalActions.MoveUp.Value);
                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.FormId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();
                await Send(command);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Create
    [HttpGet]
    [Route("/admin/forms/create")]
    public IActionResult Create()
    {
        var viewModel = new CreateFormVersionViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [Route("/admin/forms/create")]
    public async Task<IActionResult> Create(CreateFormVersionViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid) return View(viewModel);
            var command = new CreateFormVersionCommand
            {
                Title = viewModel.Name,
                Description = viewModel.Description,
            };

            if (viewModel.AdditionalFormActions.UpdateDescriptionPreview)
            {
                viewModel.DescriptionPreview = MarkdownHelper.ToGovUkHtml(viewModel.Description);
                return View(viewModel);
            }

            var response = await Send(command);
            return RedirectToAction(nameof(Edit), new { formVersionId = response.Id });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(viewModel);
        }
    }
    #endregion

    #region Edit
    [HttpGet]
    [Route("/admin/forms/{formVersionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId)
    {
        var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
        var response = await Send(formVersionQuery);

        var viewModel = EditFormVersionViewModel.Map(response);

        ShowNotificationIfKeyExists(UpdateKeys.FormUpdated.ToString(), ViewNotificationMessageType.Success, "The form has been updated.");
        ShowNotificationIfKeyExists(UpdateKeys.FormPublished.ToString(), ViewNotificationMessageType.Success, "The form has been published.");

        return View(viewModel);
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}")]
    public async Task<IActionResult> Edit(EditFormVersionViewModel editFormVersionViewModel)
    {
        try
        {
            if (editFormVersionViewModel.AdditionalFormActions.Publish != default)
            {
                var command = new PublishFormVersionCommand(editFormVersionViewModel.Id);
                await Send(command);
                TempData[UpdateKeys.FormPublished.ToString()] = true;

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

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.SectionId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();

            }
            else if (editFormVersionViewModel.AdditionalFormActions.MoveDown != default)
            {
                var command = new MoveSectionDownCommand()
                {
                    FormVersionId = editFormVersionViewModel.Id,
                    SectionId = editFormVersionViewModel.AdditionalFormActions.MoveDown ?? Guid.Empty,
                };
                await Send(command);

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.SectionId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();

            }
            else if (editFormVersionViewModel.AdditionalFormActions.UpdateDescriptionPreview)
            {

                var formVersionQuery = new GetFormVersionByIdQuery(editFormVersionViewModel.Id);
                var response = await Send(formVersionQuery);

                var viewModel = EditFormVersionViewModel.Map(response);

                viewModel.Title = editFormVersionViewModel.Title;
                viewModel.Description = editFormVersionViewModel.Description;
                viewModel.DescriptionHTML = MarkdownHelper.ToGovUkHtml(viewModel.Description);
                ViewBag.AutoFocusOnUpdateDescriptionButton = true;



                return View(viewModel);
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

                TempData[UpdateKeys.FormUpdated.ToString()] = true;
            }
            return RedirectToAction(nameof(Edit), new { formVersionId = editFormVersionViewModel.Id });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(editFormVersionViewModel);
        }
    }
    #endregion

    #region Delete
    [Route("/admin/forms/{formVersionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId)
    {
        var query = new GetFormVersionByIdQuery(formVersionId);
        var response = await Send(query);
        return View(new DeleteFormViewModel()
        {
            FormVersionId = formVersionId,
            FormId = response.FormId,
            Title = response.Title
        });
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/delete")]
    public async Task<IActionResult> Delete(DeleteFormViewModel model)
    {
        try
        {
            var command = new DeleteFormCommand(model.FormId);
            await Send(command);

            TempData[UpdateKeys.FormDeleted.ToString()] = true;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion
}