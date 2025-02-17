using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using System.Reflection;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class RoutesController : ControllerBase
{
    public RoutesController(IMediator mediator, ILogger<RoutesController> logger) : base(mediator, logger)
    {
    }

    [HttpGet()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(Guid formVersionId, Guid questionId, Guid sectionId, Guid pageId)
    {
        try
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
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(CreateRouteViewModel model)
    {
        try
        {
            var command = CreateRouteViewModel.MapToCommand(model);

            var response = await Send(command);
            return RedirectToAction(nameof(List), new { formVersionId = model.FormVersionId });
        }
        catch
        {
            return View(model);
        }
    }
    [HttpGet()]
    [Route("forms/{formVersionId}/routes/choose-section-page")]
    public async Task<IActionResult> ChooseSection(Guid formVersionId)
    {
        try
        {
            var query = new GetAvailableSectionsAndPagesForRoutingQuery()
            {
                FormVersionId = formVersionId
            };
            var response = await Send(query);
            return View(CreateRouteChooseSectionAndPageViewModel.MapToViewModel(response, formVersionId));
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/choose-section-page")]
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
                var response = await _mediator.Send(query);
                var viewModel = CreateRouteChooseSectionAndPageViewModel.MapToViewModel(response.Value, model.FormVersionId);
                viewModel.ChosenSectionId = model.ChosenSectionId;
                viewModel.ChosenPageId = model.ChosenPageId;
                return View(viewModel);
            }

            return RedirectToAction(nameof(ChooseQuestion), new { formVersionId = model.FormVersionId, sectionId = model.ChosenSectionId, pageId = model.ChosenPageId });
        }
        catch
        {
            return View(model);
        }
    }

    [HttpGet()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        try
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
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
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
        catch
        {
            return View(model);
        }
    }

    [HttpGet()]
    [Route("forms/{formVersionId}/routes")]
    public async Task<IActionResult> List(Guid formVersionId)
    {
        try
        {
            var query = new GetRoutingInformationForFormQuery()
            {
                FormVersionId = formVersionId,

            };
            var response = await Send(query);

            return View(new ListRoutesViewModel()
            {
                FormVersionId = formVersionId,
                Response = response
            });
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }
}
