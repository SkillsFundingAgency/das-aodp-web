﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using System.Reflection;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class RoutesController : Controller
{
    private readonly IMediator _mediator;

    public RoutesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(Guid formVersionId, Guid questionId, Guid sectionId, Guid pageId)
    {
        var query = new GetRoutingInformationForQuestionQuery()
        {
            FormVersionId = formVersionId,
            PageId = pageId,
            QuestionId = questionId,
            SectionId = sectionId
        };
        var response = await _mediator.Send(query);
        if (!response.Success) return NotFound();


        return View(CreateRouteViewModel.MapToViewModel(response.Value, formVersionId, sectionId, pageId));
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Configure(CreateRouteViewModel model)
    {
        var command = CreateRouteViewModel.MapToCommand(model);

        var response = await _mediator.Send(command);
        if (!response.Success) return NotFound();
        return RedirectToAction(nameof(List), new { formVersionId = model.FormVersionId });
    }
    [HttpGet()]
    [Route("forms/{formVersionId}/routes/choose-section-page")]
    public async Task<IActionResult> ChooseSection(Guid formVersionId)
    {
        var query = new GetAvailableSectionsAndPagesForRoutingQuery()
        {
            FormVersionId = formVersionId
        };
        var response = await _mediator.Send(query);
        if (!response.Success) return NotFound();
        return View(CreateRouteChooseSectionAndPageViewModel.MapToViewModel(response.Value, formVersionId));
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/choose-section-page")]
    public async Task<IActionResult> ChooseSection(CreateRouteChooseSectionAndPageViewModel model)
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

    [HttpGet()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        var query = new GetAvailableQuestionsForRoutingQuery()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId
        };
        var response = await _mediator.Send(query);
        if (!response.Success) return NotFound();

        return View(CreateRouteChooseQuestionViewModel.MapToViewModel(response.Value, formVersionId, sectionId, pageId));
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/routes/sections/{sectionId}/pages/{pageId}/choose-question")]
    public async Task<IActionResult> ChooseQuestion(CreateRouteChooseQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var query = new GetAvailableQuestionsForRoutingQuery()
            {
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                PageId = model.PageId
            };
            var response = await _mediator.Send(query);
            return View(CreateRouteChooseQuestionViewModel.MapToViewModel(response.Value, model.FormVersionId, model.SectionId, model.PageId));
        }

        return RedirectToAction(nameof(Configure), new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.ChosenQuestionId });

    }

    [HttpGet()]
    [Route("forms/{formVersionId}/routes")]
    public async Task<IActionResult> List(Guid formVersionId)
    {
        var query = new GetRoutingInformationForFormQuery()
        {
            FormVersionId = formVersionId,

        };
        var response = await _mediator.Send(query);
        if (!response.Success) return NotFound();

        return View(new ListRoutesViewModel()
        {
            FormVersionId = formVersionId,
            Response = response.Value
        });
    }
}
