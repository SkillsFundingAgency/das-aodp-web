using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.FormBuilder.Section;
using static SFA.DAS.AODP.Web.Helpers.ListHelper.OrderButtonHelper;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;


namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

[Area("Admin")]
[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
public class SectionsController : ControllerBase
{
    private const string SectionUpdatedKey = nameof(SectionUpdatedKey);

    public SectionsController(IMediator mediator, ILogger<SectionsController> logger) : base(mediator, logger)
    {
    }

    #region Create
    [Route("/admin/forms/{formVersionId}/sections/create")]
    public async Task<IActionResult> Create(Guid formVersionId)
    {
        return View(new CreateSectionViewModel { FormVersionId = formVersionId });
    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/sections/create")]
    public async Task<IActionResult> Create(CreateSectionViewModel model)
    {
        try
        {
            var command = new CreateSectionCommand()
            {
                FormVersionId = model.FormVersionId,
                Title = model.Title
            };

            var response = await Send(command);
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = response.Id });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Edit
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId)
    {
        var sectionQuery = new GetSectionByIdQuery(sectionId, formVersionId);
        var response = await Send(sectionQuery);

        ShowNotificationIfKeyExists(SectionUpdatedKey, ViewNotificationMessageType.Success, "The section has been updated.");

        return View(EditSectionViewModel.Map(response));

    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}")]
    public async Task<IActionResult> Edit(EditSectionViewModel model)
    {
        try
        {
            if (model.AdditionalActions.MoveUp != default)
            {
                var command = new MovePageUpCommand()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.AdditionalActions.MoveUp ?? Guid.Empty,
                };
                await Send(command);

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.PageId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();

                return await Edit(model.FormVersionId, model.SectionId);
            }
            else if (model.AdditionalActions.MoveDown != default)
            {
                var command = new MovePageDownCommand()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.AdditionalActions.MoveDown ?? Guid.Empty,
                };
                await _mediator.Send(command);

                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.PageId.ToString();
                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();

                return await Edit(model.FormVersionId, model.SectionId);
            }
            else
            {
                var command = new UpdateSectionCommand()
                {
                    FormVersionId = model.FormVersionId,
                    Title = model.Title,
                    Id = model.SectionId
                };

                var response = await _mediator.Send(command);
                TempData[SectionUpdatedKey] = true;
                return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId });
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Delete
    [HttpGet]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> Delete(Guid sectionId, Guid formVersionId)
    {
        var query = new GetSectionByIdQuery(sectionId, formVersionId);
        var response = await Send(query);
        return View(new DeleteSectionViewModel()
        {
            Title = response.Title,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            HasAssociatedRoutes = response.HasAssociatedRoutes
        });

    }

    [HttpPost, ActionName("Delete")]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(DeleteSectionViewModel model)
    {
        try
        {
            var command = new DeleteSectionCommand()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId
            };
            var deleteResponse = await _mediator.Send(command);
            return RedirectToAction("Edit", "Forms", new { formVersionId = model.FormVersionId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion
}