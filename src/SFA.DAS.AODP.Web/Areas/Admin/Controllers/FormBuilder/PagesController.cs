using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;
using static SFA.DAS.AODP.Web.Helpers.ListHelper.OrderButtonHelper;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

[Area("Admin")]
[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
public class PagesController : ControllerBase
{
    private const string PageUpdatedKey = nameof(PageUpdatedKey);

    public PagesController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
    {
    }

    #region Create
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/create")]
    public async Task<IActionResult> Create(Guid formVersionId, Guid sectionId)
    {
        return View(new CreatePageViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId
        });
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/create")]
    public async Task<IActionResult> Create(CreatePageViewModel model)
    {
        var command = new CreatePageCommand()
        {
            SectionId = model.SectionId,
            FormVersionId = model.FormVersionId,
            Title = model.Title

        };

        var response = await Send(command);
        return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = response.Id });
    }
    #endregion

    #region Edit
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}")]
    public async Task<IActionResult> Edit(Guid pageId, Guid sectionId, Guid formVersionId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId, formVersionId);
        var response = await Send(query);

        ShowNotificationIfKeyExists(PageUpdatedKey, ViewNotificationMessageType.Success, "The page has been updated.");

        return View(EditPageViewModel.Map(response, formVersionId));
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}")]
    public async Task<IActionResult> Edit(EditPageViewModel model)
    {
        try
        {
            if (model.AdditionalFormActions.MoveUp != default)
            {
                var command = new MoveQuestionUpCommand()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.PageId,
                    QuestionId = model.AdditionalFormActions.MoveUp ?? Guid.Empty
                };
                await Send(command);

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.QuestionId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();

                return await Edit(model.PageId, model.SectionId, model.FormVersionId);
            }
            else if (model.AdditionalFormActions.MoveDown != default)
            {
                var command = new MoveQuestionDownCommand()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.PageId,
                    QuestionId = model.AdditionalFormActions.MoveDown ?? Guid.Empty
                };
                await Send(command);

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.QuestionId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();

                return await Edit(model.PageId, model.SectionId, model.FormVersionId);
            }
            else
            {
                var command = new UpdatePageCommand()
                {
                    Title = model.Title,
                    SectionId = model.SectionId,
                    FormVersionId = model.FormVersionId,
                    Id = model.PageId
                };

                await Send(command);

                TempData[PageUpdatedKey] = true;

                return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Preview
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/preview")]
    public async Task<IActionResult> Preview(Guid pageId, Guid sectionId, Guid formVersionId)
    {
        var query = new GetPagePreviewByIdQuery(pageId, sectionId, formVersionId);
        var response = await Send(query);

        return View(new PreviewPageViewModel()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            Value = response
        });
    }
    #endregion

    #region Delete
    [HttpGet]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        var query = new GetPageByIdQuery(pageId, sectionId, formVersionId);
        var response = await Send(query);
        return View(new DeletePageViewModel()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            Title = response.Title,
            HasAssociatedRoutes = response.HasAssociatedRoutes
        });
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeletePageViewModel model)
    {
        try
        {
            var command = new DeletePageCommand()
            {
                PageId = model.PageId,
                SectionId = model.SectionId,
                FormVersionId = model.FormVersionId
            };

            await Send(command);
            return RedirectToAction("Edit", "Sections", new { formVersionId = model.FormVersionId, sectionId = model.SectionId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion
}
