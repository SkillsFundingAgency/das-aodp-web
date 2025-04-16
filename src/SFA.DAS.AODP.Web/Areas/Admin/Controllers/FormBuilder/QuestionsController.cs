using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.Markdown;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;


namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

[Area("Admin")]
[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
public class QuestionsController : ControllerBase
{
    private const string QuestionUpdatedKey = nameof(QuestionUpdatedKey);

    private readonly FormBuilderSettings _formBuilderSettings;

    public QuestionsController(IMediator mediator, ILogger<QuestionsController> logger, IOptions<FormBuilderSettings> formBuilderSettings) : base(mediator, logger)
    {
        _formBuilderSettings = formBuilderSettings.Value;
    }

    #region Create

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/add-question")]
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
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/add-question")]
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
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Edit

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {

        var query = new GetQuestionByIdQuery()
        {
            PageId = pageId,
            SectionId = sectionId,
            FormVersionId = formVersionId,
            QuestionId = questionId
        };
        var response = await Send(query);

        var map = EditQuestionViewModel.MapToViewModel(response, formVersionId, sectionId, _formBuilderSettings);

        ShowNotificationIfKeyExists(QuestionUpdatedKey, ViewNotificationMessageType.Success, "The question has been updated.");

        return View(map);

    }

    [HttpPost()]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    public async Task<IActionResult> Edit(EditQuestionViewModel model)
    {
        try
        {
            if (model.FileUpload != null) model.FileUpload.FileTypes = _formBuilderSettings.UploadFileTypesAllowed;

            if (model.AdditionalActions?.UpdateDescriptionPreview == true)
            {
                model.HelperHTML = MarkdownHelper.ToGovUkHtml(model.Helper);
                ViewBag.AutoFocusOnUpdateDescriptionButton = true;
                return View(model);
            }

            if (model.Options.AdditionalFormActions.AddOption)
            {
                model.Options.Options.Add(new()
                {
                    Order = model.Options.Options.Count > 0 ? model.Options.Options.Max(o => o.Order) + 1 : 1
                });
                ViewBag.AutoFocusOnAddOptionButton = true;
                return View(model);
            }
            else if (model.Options.AdditionalFormActions.RemoveOptionIndex.HasValue)
            {
                int indexToRemove = model.Options.AdditionalFormActions.RemoveOptionIndex.Value;

                if (model.Options.Options[indexToRemove].DoesHaveAssociatedRoutes)
                {
                    ModelState.AddModelError($"Options.Options[{indexToRemove}]", "You cannot remove this option because it has associated routes.");
                    return View(model);
                }
                else
                {
                    model.Options.Options.RemoveAt(indexToRemove);
                    ViewBag.AutoFocusOnAddOptionButton = true;
                    return View(model);
                }
            }


            ValidateEditQuestionViewModel(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var command = EditQuestionViewModel.MapToCommand(model);
            await Send(command);


            if (model.AdditionalActions?.SaveAndExit == true)
            {
                TempData[QuestionUpdatedKey] = true;
                return RedirectToAction("Edit", "Pages", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
            }
            else if (model.AdditionalActions?.SaveAndAddAnother == true)
            {
                return RedirectToAction("Create", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
            }
            else
            {
                TempData[QuestionUpdatedKey] = true;
                return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
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

    [HttpGet()]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId)
    {
        var routesQuery = new GetRoutingInformationForQuestionQuery()
        {
            FormVersionId = formVersionId,
            PageId = pageId,
            QuestionId = questionId,
            SectionId = sectionId
        };
        var routesResponse = await Send(routesQuery);
        if (routesResponse.Routes.Any())
        {
            ModelState.AddModelError("", "There are routes associated with this question");
        }

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

    [HttpPost()]
    [ValidateAntiForgeryToken]
    [Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}/delete")]
    public async Task<IActionResult> Delete(Guid formVersionId, Guid sectionId, Guid pageId, Guid questionId, DeleteQuestionViewModel model)
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

            return RedirectToAction("Edit", "Pages", new { formVersionId, sectionId, pageId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
    #endregion

    #region Validation
    private void ValidateEditQuestionViewModel(EditQuestionViewModel editQuestionViewModel)
    {
        switch (editQuestionViewModel.Type)
        {
            case AODP.Models.Forms.QuestionType.File:
            {
                if (editQuestionViewModel.FileUpload.NumberOfFiles > _formBuilderSettings.MaxUploadNumberOfFiles)
                {
                    ModelState.AddModelError("FileUpload.NumberOfFiles", $"The number of files cannot be greater than {_formBuilderSettings.MaxUploadNumberOfFiles}");
                }
                break;
            }
            case AODP.Models.Forms.QuestionType.Text:
            {
                if (editQuestionViewModel.TextInput.MinLength > editQuestionViewModel.TextInput.MaxLength)
                {
                    ModelState.AddModelError("TextInput.MinLength", $"The minimum length cannot be greater than {editQuestionViewModel.TextInput.MaxLength}");
                }
                if (editQuestionViewModel.TextInput.MaxLength < editQuestionViewModel.TextInput.MinLength)
                {
                    ModelState.AddModelError("TextInput.MaxLength", $"The maximum length cannot be less than {editQuestionViewModel.TextInput.MinLength}");
                }
                break;
            }
            case AODP.Models.Forms.QuestionType.Number:
            {
                if (editQuestionViewModel.NumberInput.LessThanOrEqualTo >= editQuestionViewModel.NumberInput.GreaterThanOrEqualTo)
                {
                    ModelState.AddModelError("NumberInput.LessThanOrEqualTo", $"The number provided cannot be greater than or equal to {editQuestionViewModel.NumberInput.GreaterThanOrEqualTo}");
                }
                if (editQuestionViewModel.NumberInput.GreaterThanOrEqualTo <= editQuestionViewModel.NumberInput.LessThanOrEqualTo)
                {
                    ModelState.AddModelError("NumberInput.GreaterThanOrEqualTo", $"The number provided cannot be less than or equal to {editQuestionViewModel.NumberInput.LessThanOrEqualTo}");
                }
                break;
            }
            case AODP.Models.Forms.QuestionType.MultiChoice:
            {
                if (editQuestionViewModel.Checkbox.MinNumberOfOptions > editQuestionViewModel.Checkbox.MaxNumberOfOptions)
                {
                    ModelState.AddModelError("Checkbox.MinNumberOfOptions", $"The number of checkbox options must be less than {editQuestionViewModel.Checkbox.MaxNumberOfOptions}");
                }
                if (editQuestionViewModel.Checkbox.MaxNumberOfOptions < editQuestionViewModel.Checkbox.MinNumberOfOptions)
                {
                    ModelState.AddModelError("Checkbox.MaxNumberOfOptions", $"The number of checkbox options must be greater than {editQuestionViewModel.Checkbox.MinNumberOfOptions}");
                }
                break;
            }
            case AODP.Models.Forms.QuestionType.Date:
            {
                if (editQuestionViewModel.DateInput.LessThanOrEqualTo >= editQuestionViewModel.DateInput.GreaterThanOrEqualTo)
                {
                    ModelState.AddModelError("DateInput.LessThanOrEqualTo", $"The date provided must be earlier than {editQuestionViewModel.DateInput.GreaterThanOrEqualTo}");
                }
                if (editQuestionViewModel.DateInput.GreaterThanOrEqualTo <= editQuestionViewModel.DateInput.LessThanOrEqualTo)
                {
                    ModelState.AddModelError("DateInput.GreaterThanOrEqualTo", $"The date provided must be later than {editQuestionViewModel.DateInput.LessThanOrEqualTo}");
                }
                break;
            }
            default:
            {
                break;
            }
        }
    }
    #endregion
}
