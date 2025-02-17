using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class QuestionsController : Controller
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Create

    [HttpGet()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/add-question")]
    public async Task<IActionResult> Create(Guid formVersionId, Guid sectionId, Guid pageId)
    {
        return View(new CreateQuestionViewModel
        {
            PageId = pageId,
            FormVersionId = formVersionId,
            SectionId = sectionId
        });
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/add-question")]
    public async Task<IActionResult> Create(CreateQuestionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var command = new CreateQuestionCommand()
        {
            SectionId = model.SectionId,
            FormVersionId = model.FormVersionId,
            Required = model.Required,
            Title = model.Title,
            PageId = model.PageId,
            Type = model.QuestionType.ToString()
        };

        var response = await _mediator.Send(command);
        return RedirectToAction(nameof(Edit), new
        {
            formVersionId = model.FormVersionId,
            sectionId = model.SectionId,
            pageId = model.PageId,
            questionId = response.Value.Id
        });
    }
    #endregion

    #region Edit

    [HttpGet()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        var query = new GetQuestionByIdQuery()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            QuestionId = questionId
        };
        var response = await _mediator.Send(query);
        if (response.Value == null) return NotFound();

        var map = EditQuestionViewModel.MapToViewModel(response.Value, formVersionId, sectionId);
        return View(map);

    }

    [HttpPost()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(EditQuestionViewModel model)
    {
        if (model.Options.AdditionalFormActions.AddOption)
        {
            model.Options.Options.Add(new());
            return View(model);
        }
        else if (model.Options.AdditionalFormActions.RemoveOptionIndex.HasValue)
        {
            int indexToRemove = model.Options.AdditionalFormActions.RemoveOptionIndex.Value;
            if (indexToRemove >= 0 && indexToRemove < model.Options.Options.Count)
            {
                if (model.Options.Options[indexToRemove].DoesHaveAssociatedRoutes)
                {
                    ModelState.AddModelError($"RadioButton.MultiChoice[{indexToRemove}]", "You cannot remove this option because it has associated routes.");
                }
                else
                {
                    model.Options.Options.RemoveAt(indexToRemove);
                }
            }
            return View(model);
        }

        var command = EditQuestionViewModel.MapToCommand(model);
        var response = await _mediator.Send(command);


        return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
    }
    #endregion

    #region Delete

    [HttpGet()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        var routesQuery = new GetRoutingInformationForQuestionQuery()
        {
            FormVersionId = formVersionId,
            PageId = pageId,
            QuestionId = questionId,
            SectionId = sectionId
        };
        var routesResponse = await _mediator.Send(routesQuery);
        if (routesResponse.Value.Routes.Any())
        {
            ModelState.AddModelError("", "There are routes associated with this question");
        }

        // Instead of the above, add Routes to the GetQuestionByIdQueryResponse???
        var query = new GetQuestionByIdQuery()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            QuestionId = questionId
        };

        var response = await _mediator.Send(query);

        if (response.Value == null) return NotFound();

        var vm = DeleteQuestionViewModel.MapToViewModel(response.Value, formVersionId, sectionId);

        return View(vm);
    }

    [HttpPost()]
    [ValidateAntiForgeryToken]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        var command = new DeleteQuestionCommand
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            QuestionId = questionId
        };

        var deleteQuestionResponse = await _mediator.Send(command);

        return RedirectToAction("Edit", "Pages", new { formVersionId = formVersionId, sectionId = sectionId, pageId = pageId });
    }
    #endregion
}
