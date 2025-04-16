using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.Markdown;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using System.Reflection.Metadata;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class QuestionsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<QuestionsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IOptions<FormBuilderSettings>> _formBuilderSettingsMock;
    private readonly QuestionsController _controller;

    public QuestionsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<QuestionsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _formBuilderSettingsMock = _fixture.Freeze<Mock<IOptions<FormBuilderSettings>>>();
        _controller = new QuestionsController(_mediatorMock.Object, _loggerMock.Object, _formBuilderSettingsMock.Object);
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
    }

    public class DateOnlySpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(DateOnly))
            {
                return new DateOnly(2023, 1, 1); // a valid date
            }

            return new NoSpecimen();
        }
    }

    [Fact]
    public async Task Validate_File_NumFilesGreaterThanMaxUploadNumFiles_OnSuccess()
    {
        // Arrange
        _formBuilderSettingsMock.Object.Value.MaxUploadNumberOfFiles = 1;

        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.File,
            FileUpload = new()
            {
                NumberOfFiles = 1
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("FileUpload.NumberOfFiles", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_File_NumFilesGreaterThanMaxUploadNumFiles_OnFailure()
    {
        // Arrange
        _formBuilderSettingsMock.Object.Value.MaxUploadNumberOfFiles = 1;

        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.File,
            FileUpload = new()
            {
                NumberOfFiles = 2
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(1, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("FileUpload.NumberOfFiles", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Text_Length_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Text,
            TextInput = new()
            {
                MinLength = 1,
                MaxLength = 2
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("TextInput.MinLength", viewResult.ViewData.ModelState.Keys);
        Assert.DoesNotContain("TextInput.MaxLength", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Text_Length_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Text,
            TextInput = new()
            {
                MinLength = 2,
                MaxLength = 1
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(2, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("TextInput.MinLength", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("TextInput.MaxLength", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Number_Length_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {
                LessThanOrEqualTo = 1,
                GreaterThanOrEqualTo = 3
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("NumberInput.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
        Assert.DoesNotContain("NumberInput.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Number_Length_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {
                LessThanOrEqualTo = 3,
                GreaterThanOrEqualTo = 1
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(2, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("NumberInput.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("NumberInput.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

[Fact]
    public async Task Validate_Checkbox_NumOptions_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Checkbox = new()
            {
                MinNumberOfOptions = 1,
                MaxNumberOfOptions = 2
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("Checkbox.MinNumberOfOptions", viewResult.ViewData.ModelState.Keys);
        Assert.DoesNotContain("Checkbox.MaxNumberOfOptions", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]    
    public async Task Validate_Checkbox_NumOptions_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Checkbox = new()
            {
                MinNumberOfOptions = 2,
                MaxNumberOfOptions = 1
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(2, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("Checkbox.MinNumberOfOptions", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("Checkbox.MaxNumberOfOptions", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Date_Value_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Date,
            DateInput = new()
            {
                LessThanOrEqualTo = DateOnly.MinValue,
                GreaterThanOrEqualTo = DateOnly.MaxValue
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("DateInput.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
        Assert.DoesNotContain("DateInput.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Date_Value_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Date,
            DateInput = new()
            {
                LessThanOrEqualTo = DateOnly.MaxValue,
                GreaterThanOrEqualTo = DateOnly.MinValue
            }
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQuestionByIdQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<EditQuestionViewModel>(viewResult.ViewData.Model);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(2, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("DateInput.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("DateInput.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }
    //var query = new GetQuestionByIdQuery()
    //{
    //    PageId = pageId,
    //    SectionId = sectionId,
    //    FormVersionId = formVersionId,
    //    QuestionId = questionId
    //};
    //var response = await Send(query);

    //var map = EditQuestionViewModel.MapToViewModel(response, formVersionId, sectionId, _formBuilderSettings);

    //ShowNotificationIfKeyExists(QuestionUpdatedKey, ViewNotificationMessageType.Success, "The question has been updated.");

    //    return View(map);

    //[HttpPost()]
    //[Route("/admin/forms/{formVersionId}/sections/{sectionId}/pages/{pageId}/questions/{questionId}")]
    //public async Task<IActionResult> Edit(EditQuestionViewModel model)
    //{
    //    try
    //    {
    //        if (model.FileUpload != null) model.FileUpload.FileTypes = _formBuilderSettings.UploadFileTypesAllowed;

    //        if (model.AdditionalActions?.UpdateDescriptionPreview == true)
    //        {
    //            model.HelperHTML = MarkdownHelper.ToGovUkHtml(model.Helper);
    //            ViewBag.AutoFocusOnUpdateDescriptionButton = true;
    //            return View(model);
    //        }

    //        if (model.Options.AdditionalFormActions.AddOption)
    //        {
    //            model.Options.Options.Add(new()
    //            {
    //                Order = model.Options.Options.Count > 0 ? model.Options.Options.Max(o => o.Order) + 1 : 1
    //            });
    //            ViewBag.AutoFocusOnAddOptionButton = true;
    //            return View(model);
    //        }
    //        else if (model.Options.AdditionalFormActions.RemoveOptionIndex.HasValue)
    //        {
    //            int indexToRemove = model.Options.AdditionalFormActions.RemoveOptionIndex.Value;

    //            if (model.Options.Options[indexToRemove].DoesHaveAssociatedRoutes)
    //            {
    //                ModelState.AddModelError($"Options.Options[{indexToRemove}]", "You cannot remove this option because it has associated routes.");
    //                return View(model);
    //            }
    //            else
    //            {
    //                model.Options.Options.RemoveAt(indexToRemove);
    //                ViewBag.AutoFocusOnAddOptionButton = true;
    //                return View(model);
    //            }
    //        }


    //        ValidateEditQuestionViewModel(model);
    //        if (!ModelState.IsValid)
    //        {
    //            return View(model);
    //        }


    //        var command = EditQuestionViewModel.MapToCommand(model);
    //        await Send(command);


    //        if (model.AdditionalActions?.SaveAndExit == true)
    //        {
    //            TempData[QuestionUpdatedKey] = true;
    //            return RedirectToAction("Edit", "Pages", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
    //        }
    //        else if (model.AdditionalActions?.SaveAndAddAnother == true)
    //        {
    //            return RedirectToAction("Create", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId });
    //        }
    //        else
    //        {
    //            TempData[QuestionUpdatedKey] = true;
    //            return RedirectToAction("Edit", new { formVersionId = model.FormVersionId, sectionId = model.SectionId, pageId = model.PageId, questionId = model.Id });
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        LogException(ex);
    //        return View(model);
    //    }



    //#region Validation
    //private void ValidateEditQuestionViewModel(EditQuestionViewModel editQuestionViewModel)
    //{
    //    switch (editQuestionViewModel.Type)
    //    {
    //        case AODP.Models.Forms.QuestionType.File:
    //            {
    //                if (editQuestionViewModel.FileUpload.NumberOfFiles > _formBuilderSettings.MaxUploadNumberOfFiles)
    //                {
    //                    ModelState.AddModelError("FileUpload.NumberOfFiles", $"The number of files cannot be greater than {_formBuilderSettings.MaxUploadNumberOfFiles}");
    //                }
    //                break;
    //            }
    //        case AODP.Models.Forms.QuestionType.Text:
    //            {
    //                if (editQuestionViewModel.TextInput.MinLength > editQuestionViewModel.TextInput.MaxLength)
    //                {
    //                    ModelState.AddModelError("TextInput.MinLength", $"The minimum length cannot be greater than {editQuestionViewModel.TextInput.MaxLength}");
    //                }
    //                if (editQuestionViewModel.TextInput.MaxLength < editQuestionViewModel.TextInput.MinLength)
    //                {
    //                    ModelState.AddModelError("TextInput.MaxLength", $"The maximum length cannot be less than {editQuestionViewModel.TextInput.MinLength}");
    //                }
    //                break;
    //            }
    //        case AODP.Models.Forms.QuestionType.Number:
    //            {
    //                if (editQuestionViewModel.NumberInput.LessThanOrEqualTo >= editQuestionViewModel.NumberInput.GreaterThanOrEqualTo)
    //                {
    //                    ModelState.AddModelError("NumberInput.LessThanOrEqualTo", $"The number provided cannot be greater than or equal to {editQuestionViewModel.NumberInput.GreaterThanOrEqualTo}");
    //                }
    //                if (editQuestionViewModel.NumberInput.GreaterThanOrEqualTo <= editQuestionViewModel.NumberInput.LessThanOrEqualTo)
    //                {
    //                    ModelState.AddModelError("NumberInput.GreaterThanOrEqualTo", $"The number provided cannot be less than or equal to {editQuestionViewModel.NumberInput.LessThanOrEqualTo}");
    //                }
    //                break;
    //            }
    //        case AODP.Models.Forms.QuestionType.MultiChoice:
    //            {
    //                if (editQuestionViewModel.Checkbox.MinNumberOfOptions > editQuestionViewModel.Checkbox.MaxNumberOfOptions)
    //                {
    //                    ModelState.AddModelError("Checkbox.MinNumberOfOptions", $"The number of checkbox options must be less than {editQuestionViewModel.Checkbox.MaxNumberOfOptions}");
    //                }
    //                if (editQuestionViewModel.Checkbox.MaxNumberOfOptions < editQuestionViewModel.Checkbox.MinNumberOfOptions)
    //                {
    //                    ModelState.AddModelError("Checkbox.MaxNumberOfOptions", $"The number of checkbox options must be greater than {editQuestionViewModel.Checkbox.MinNumberOfOptions}");
    //                }
    //                break;
    //            }
    //        case AODP.Models.Forms.QuestionType.Date:
    //            {
    //                if (editQuestionViewModel.DateInput.LessThanOrEqualTo >= editQuestionViewModel.DateInput.GreaterThanOrEqualTo)
    //                {
    //                    ModelState.AddModelError("DateInput.LessThanOrEqualTo", $"The date provided must be earlier than {editQuestionViewModel.DateInput.GreaterThanOrEqualTo}");
    //                }
    //                if (editQuestionViewModel.DateInput.GreaterThanOrEqualTo <= editQuestionViewModel.DateInput.LessThanOrEqualTo)
    //                {
    //                    ModelState.AddModelError("DateInput.GreaterThanOrEqualTo", $"The date provided must be later than {editQuestionViewModel.DateInput.LessThanOrEqualTo}");
    //                }
    //                break;
    //            }
    //    }
    //}
    //#endregion
}