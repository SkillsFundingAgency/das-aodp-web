using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder;

public class QuestionsController : ControllerBase
{
    private readonly FormBuilderSettings _formBuilderSettings;

    public QuestionsController(IMediator mediator, ILogger<FormsController> logger, IOptions<FormBuilderSettings> formBuilderSettings) : base(mediator, logger)
    {
        _formBuilderSettings = formBuilderSettings.Value;
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


        for (int i = 0; i < response.Value.Options.Count; i++)
        {
            if (TempData.TryGetValue($"MultiChoiceError_{i}", out var error))
            {
                ModelState.AddModelError($"RadioButton.MultiChoice[{i}]", error?.ToString() ?? string.Empty);
            }
        }

        var map = EditQuestionViewModel.MapToViewModel(response.Value, formVersionId, sectionId,_formBuilderSettings);
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
            ValidateEditQuestionViewModel(model);
            if (!ModelState.IsValid)
            {
                if (model.FileUpload != null) model.FileUpload.FileTypes = _formBuilderSettings.UploadFileTypesAllowed;
                return View(model);
            }

            if (model.Options.AdditionalFormActions.AddOption)
            {
                model.Options.Options.Add(new());
                return View(model);
            }
            else if (model.Options.AdditionalFormActions.RemoveOptionIndex.HasValue)
            {
                int indexToRemove = model.Options.AdditionalFormActions.RemoveOptionIndex.Value;

                 if (model.Options.Options[indexToRemove].DoesHaveAssociatedRoutes)
                {
                    TempData[$"MultiChoiceError_{indexToRemove}"] = "You cannot remove this option because it has associated routes.";
                                return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
                }
                else
                {
                    model.Options.Options.RemoveAt(indexToRemove);
                    return View(model);
                }
            }


            var command = EditQuestionViewModel.MapToCommand(model);
            var response = await _mediator.Send(command);

            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
        }
        catch
        {
            return View(model);
        }
               
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
        try       
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

    #region Validation
    private void ValidateEditQuestionViewModel(EditQuestionViewModel editQuestionViewModel)
    {
        if (editQuestionViewModel.Type == AODP.Models.Forms.QuestionType.File)
        {
            if (editQuestionViewModel.FileUpload.MaxSize > _formBuilderSettings.MaxUploadFileSize)
            {
                ModelState.AddModelError("FileUpload.MaxSize", $"The file upload size cannot be greater than {_formBuilderSettings.MaxUploadFileSize}");
            }
            if (editQuestionViewModel.FileUpload.NumberOfFiles > _formBuilderSettings.MaxUploadNumberOfFiles)
            {
                ModelState.AddModelError("FileUpload.NumberOfFiles", $"The number of files cannot be greater than {_formBuilderSettings.MaxUploadNumberOfFiles}");
            }
        }
    }
    #endregion
}
