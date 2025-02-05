using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Web.Models.Question;

namespace SFA.DAS.AODP.Web.Controllers;
public class QuestionsController : Controller
{
    private readonly IMediator _mediator;

    public QuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult Index()
    {
        return View();
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
        if (model.RadioButton.AdditionalFormActions.AddOption)
        {
            model.RadioButton.MultiChoice.Add(new());
            return View(model);
        }
        else if (model.RadioButton.AdditionalFormActions.RemoveOption != default)
        {
            model.RadioButton.MultiChoice = model.RadioButton.MultiChoice.Where(o => o.Id != model.RadioButton.AdditionalFormActions.RemoveOption).ToList();
            return View(model);

        }
        var command = EditQuestionViewModel.MapToCommand(model);
        var response = await _mediator.Send(command);

        return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
    }



    //[HttpPost]
    //public async Task<IActionResult> Create(Page page)
    //{
    //    var command = new CreatePageCommand
    //    {
    //        SectionId = page.SectionId,
    //        Title = page.Title,
    //        Description = page.Description,
    //        Order = page.Order,
    //        NextPageId = page.NextPageId,
    //    };

    //    var createPageResponse = await _mediator.Send(command);
    //    return RedirectToAction("Edit", "Section", new { id = page.SectionId });
    //}
    #endregion


    #region Delete
    //public async Task<IActionResult> Delete(Guid id)
    //{
    //    var pageQuery = await _mediator.Send(new GetPageByIdQuery { Id = id });
    //    if (pageQuery.Data == null) return NotFound();
    //    return View(pageQuery.Data);
    //}

    //[HttpPost, ActionName("Delete")]
    //public async Task<IActionResult> DeleteConfirmed(Guid id)
    //{

    //    var command = new DeletePageCommand { PageId = id };
    //    var deletePageResponse = await _mediator.Send(command);
    //    return RedirectToAction("Edit", "Section", new { id = pageQuery.Data.SectionId });

    //    return NotFound();
    //}
    #endregion
}
