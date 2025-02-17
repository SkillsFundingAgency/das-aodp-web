using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class QuestionsController : ControllerBase
{

    public QuestionsController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
    {
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
        try
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

            var response = await Send(command);
            return RedirectToAction(nameof(Edit), new
            {
                formVersionId = model.FormVersionId,
                sectionId = model.SectionId,
                pageId = model.PageId,
                questionId = response.Id
            });
        }
        catch
        {
            return View(model);
        }
    }
    #endregion

    #region Edit

    [HttpGet()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        try
        {
            var query = new GetQuestionByIdQuery()
            {
                PageId = pageId,
                SectionId = sectionId,
                FormVersionId = formVersionId,
                QuestionId = questionId
            };
            var response = await Send(query);

            var map = EditQuestionViewModel.MapToViewModel(response, formVersionId, sectionId);
            return View(map);
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(EditQuestionViewModel model)
    {
        try
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
                    model.Options.Options.RemoveAt(indexToRemove);
                }
                return View(model);
            }
            var command = EditQuestionViewModel.MapToCommand(model);
            var response = await Send(command);
            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
        }
        catch
        {
            return View(model);
        }
    }
    #endregion

    #region Delete

    [HttpGet()]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        try
        {
            var query = new GetQuestionByIdQuery()
            {
                PageId = pageId,
                SectionId = sectionId,
                FormVersionId = formVersionId,
                QuestionId = questionId
            };

            var response = await Send(query);

            var vm = DeleteQuestionViewModel.MapToViewModel(response, formVersionId, sectionId);

            return View(vm);
        }
        catch
        {
            return Redirect("/Home/Error");
        }
    }

    [HttpPost()]
    [ValidateAntiForgeryToken]
    [Route("forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId, [FromBody] DeleteQuestionViewModel model)
    {
        try
        {
            var command = new DeleteQuestionCommand
            {
                PageId = pageId,
                SectionId = sectionId,
                FormVersionId = formVersionId,
                QuestionId = questionId
            };

            await Send(command);

            return RedirectToAction("Edit", "Pages", new { formVersionId = formVersionId, sectionId = sectionId, pageId = pageId });
        }
        catch
        {
            return View(model);
        }
    }
    #endregion
}
