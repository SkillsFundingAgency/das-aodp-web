using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

[Area("Admin")]
[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
public class RoutesController : ControllerBase
{
    public enum UpdateKeys { RouteDeleted }
    public RoutesController(IMediator mediator, ILogger<RoutesController> logger) : base(mediator, logger)
    {
    }

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(Guid formVersionId, Guid questionId, Guid sectionId, Guid pageId)
    {
        var query = new GetRoutingInformationForQuestionQuery()
        {
            FormVersionId = formVersionId,
            PageId = pageId,
            QuestionId = questionId,
            SectionId = sectionId
        };
        var response = await Send(query);

        return View(CreateRouteViewModel.MapToViewModel(response, formVersionId, sectionId, pageId));

    }

    [HttpPost()]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(CreateRouteViewModel model)
    {
        try
        {
            var command = CreateRouteViewModel.MapToCommand(model);

            var response = await Send(command);
            return RedirectToAction(nameof(List), new { formVersionId = model.FormVersionId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/routes/choose-section-page")]
    public async Task<IActionResult> ChooseSection(Guid formVersionId)
    {
        var query = new GetAvailableSectionsAndPagesForRoutingQuery()
        {
            FormVersionId = formVersionId
        };
        var response = await Send(query);
        return View(CreateRouteChooseSectionAndPageViewModel.MapToViewModel(response, formVersionId));

    }

    [HttpPost()]
    [Route("/admin/forms/{formVersionId}/routes/choose-section-page")]
    public async Task<IActionResult> ChooseSection(CreateRouteChooseSectionAndPageViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var query = new GetAvailableSectionsAndPagesForRoutingQuery()
                {
                    FormVersionId = model.FormVersionId
                };
                var response = await Send(query);
                var viewModel = CreateRouteChooseSectionAndPageViewModel.MapToViewModel(response, model.FormVersionId);
                viewModel.ChosenSectionId = model.ChosenSectionId;
                viewModel.ChosenPageId = model.ChosenPageId;
                return View(viewModel);
            }

            return RedirectToAction(nameof(ChooseQuestion), new { formVersionId = model.FormVersionId, sectionId = model.ChosenSectionId, pageId = model.ChosenPageId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        var query = new GetAvailableQuestionsForRoutingQuery()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId
        };
        var response = await Send(query);

        return View(CreateRouteChooseQuestionViewModel.MapToViewModel(response, formVersionId, sectionId, pageId));

    }

    [HttpPost()]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(CreateRouteChooseQuestionViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var query = new GetAvailableQuestionsForRoutingQuery()
                {
                    FormVersionId = model.FormVersionId,
                    SectionId = model.SectionId,
                    PageId = model.PageId
                };
                var response = await Send(query);
                return View(CreateRouteChooseQuestionViewModel.MapToViewModel(response, model.FormVersionId, model.SectionId, model.PageId));
            }

            return RedirectToAction(nameof(Configure), new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.ChosenQuestionId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/routes")]
    public async Task<IActionResult> List(Guid formVersionId)
    {
        var query = new GetRoutingInformationForFormQuery()
        {
            FormVersionId = formVersionId,

        };
        var response = await Send(query);

        ShowNotificationIfKeyExists(UpdateKeys.RouteDeleted.ToString(), ViewNotificationMessageType.Success, "The route has been deleted.");

        return View(new ListRoutesViewModel()
        {
            FormVersionId = formVersionId,
            Response = response
        });

    }

    #region Delete
    [HttpGet]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        return View(new DeleteRouteViewModel()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            QuestionId = questionId
        });

    }

    [HttpPost]
    [Route("/admin/forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(DeleteRouteViewModel model)
    {
        try
        {
            var command = new DeleteRouteCommand()
            {
                PageId = model.PageId,
                SectionId = model.SectionId,
                FormVersionId = model.FormVersionId,
                QuestionId = model.QuestionId
            };

            await Send(command);

            TempData[UpdateKeys.RouteDeleted.ToString()] = true;
            return RedirectToAction(nameof(List), new { formVersionId = model.FormVersionId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion
}
