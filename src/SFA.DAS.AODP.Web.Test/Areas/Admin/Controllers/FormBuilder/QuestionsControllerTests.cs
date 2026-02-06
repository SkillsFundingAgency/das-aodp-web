using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;
using System.ComponentModel.DataAnnotations;

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

        SetupValidationServices(new FormBuilderSettings
        {
            MaxUploadNumberOfFiles = 3,
            UploadFileTypesAllowed = new List<string>() { "pdf", "doc"}
        });
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
    public async Task Delete_Get_ReturnsViewModel()
    {
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var routesResponse = new BaseMediatrResponse<GetRoutingInformationForQuestionQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetRoutingInformationForQuestionQueryResponse>()
        };


        var queryResponse = new BaseMediatrResponse<GetQuestionByIdQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetQuestionByIdQueryResponse>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoutingInformationForQuestionQuery>(), default))
                     .ReturnsAsync(routesResponse);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);
        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId, questionId);

        // Assert
        Assert.Multiple(() =>
        {
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<DeleteQuestionViewModel>(viewResult.ViewData.Model);
            Assert.Equal(formVersionId, model.FormVersionId);
            Assert.Equal(sectionId, model.SectionId);
            Assert.Equal(pageId, model.PageId);
            Assert.Equal(questionId, model.QuestionId);
            Assert.Equal(routesResponse.Value.Routes.Count, model.Routes.Count);
            Assert.Equal(queryResponse.Value.Title, model.Title);
        });
    }

    [Fact]
    public async Task Delete_Post_ReturnsRedirectToListPage()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeleteQuestionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteQuestionCommandResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Delete(model.FormVersionId, model.SectionId, model.PageId, model.QuestionId, model);

        // Assert
        Assert.Multiple(() =>
        {
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Equal("Pages", redirect.ControllerName);
        });
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeleteQuestionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteQuestionCommandResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Delete(model.FormVersionId, model.SectionId, model.PageId, model.QuestionId, model);

        // Assert
        Assert.Multiple(() =>
        {
            var viewResult = Assert.IsType<ViewResult>(result);
            var routeModel = Assert.IsAssignableFrom<DeleteQuestionViewModel>(viewResult.ViewData.Model);
            Assert.Equal(formVersionId, routeModel.FormVersionId);
            Assert.Equal(sectionId, routeModel.SectionId);
            Assert.Equal(pageId, routeModel.PageId);
            Assert.Equal(questionId, routeModel.QuestionId);
        });
    }

    [Fact]
    public async Task Edit_Post_ValidModel_RedirectsAndCallsMediator()
    {
        const int min = 1;
        const int max = 10;

        var model = _fixture.Build<EditQuestionViewModel>()
            .With(m => m.Type, QuestionType.Text)
            .With(m => m.TextInput, new EditQuestionViewModel.TextInputOptions
            {
                MinLength = min,
                MaxLength = max
            })
            .With(m => m.AdditionalActions, new EditQuestionViewModel.AdditionalFormActions())
            .With(m => m.Options, new EditQuestionViewModel.Option
            {
                AdditionalFormActions = new EditQuestionViewModel.Option.AdditionalActions(),
                Options = new List<EditQuestionViewModel.Option.OptionItem>()
            })
            .With(m => m.FileUpload, null as EditQuestionViewModel.FileUploadOptions)
            .Create();

        var response = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        response.Success = true;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default))
            .ReturnsAsync(response);

        ValidateAndPopulateModelState(model);
        var result = await _controller.Edit(model);

        Assert.Multiple(() =>
        {
            Assert.True(_controller.ModelState.IsValid);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Edit", redirect.ActionName);
            Assert.Null(redirect.ControllerName);
            Assert.Equal(model.FormVersionId, redirect.RouteValues["formVersionId"]);
            Assert.Equal(model.SectionId, redirect.RouteValues["sectionId"]);
            Assert.Equal(model.PageId, redirect.RouteValues["pageId"]);
            Assert.Equal(model.Id, redirect.RouteValues["questionId"]);
        });

        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_InvalidModelState_ReturnsViewAndDoesNotCallMediator()
    {
        const int min = 1;
        const int max = 10;

        var model = _fixture.Build<EditQuestionViewModel>()
            .With(m => m.Type, QuestionType.Text)
            .With(m => m.TextInput, new EditQuestionViewModel.TextInputOptions
            {
                MinLength = max,
                MaxLength = min
            })
            .With(m => m.AdditionalActions, new EditQuestionViewModel.AdditionalFormActions())
            .With(m => m.Options, new EditQuestionViewModel.Option
            {
                AdditionalFormActions = new EditQuestionViewModel.Option.AdditionalActions(),
                Options = new List<EditQuestionViewModel.Option.OptionItem>()
            })
            .With(m => m.FileUpload, null as EditQuestionViewModel.FileUploadOptions)
            .Create();

        ValidateAndPopulateModelState(model);
        var result = await _controller.Edit(model);

        Assert.Multiple(() =>
        {
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<EditQuestionViewModel>(viewResult.Model);

            Assert.Same(model, returnedModel);
            Assert.False(_controller.ModelState.IsValid);
        });

        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Never);
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(1, 10, null, null)]
    [InlineData(10, 10, null, null)]
    [InlineData(0, null, "TextInput.MinLength", FormBuilderValidationMessages.MinWordsMustBeGreaterThanZero)]
    [InlineData(null, 0, "TextInput.MaxLength", FormBuilderValidationMessages.MaxWordsMustBeGreaterThanZero)]
    [InlineData(0, 0, "TextInput.MinLength", FormBuilderValidationMessages.MinWordsMustBeGreaterThanZero)]
    [InlineData(5, 2, "TextInput.MaxLength", FormBuilderValidationMessages.MaxWordsMustBeGreaterThanOrEqualToMin)]
    public async Task Edit_Post_TextInput_Validation_Works_AsExpected(
    int? min,
    int? max,
    string expectedKey,
    string expectedErrorMessage)
    {
        var model = _fixture.Build<EditQuestionViewModel>()
            .With(m => m.Type, QuestionType.Text)
            .With(m => m.TextInput, new EditQuestionViewModel.TextInputOptions
            {
                MinLength = min,
                MaxLength = max
            })
            .With(m => m.AdditionalActions, new EditQuestionViewModel.AdditionalFormActions())
            .With(m => m.Options, new EditQuestionViewModel.Option
            {
                AdditionalFormActions = new EditQuestionViewModel.Option.AdditionalActions(),
                Options = new List<EditQuestionViewModel.Option.OptionItem>()
            })
            .With(m => m.FileUpload, null as EditQuestionViewModel.FileUploadOptions)
            .Create();

        ValidateAndPopulateModelState(model);
        var result = await _controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<EditQuestionViewModel>(viewResult.Model);
        Assert.Same(model, returnedModel);

        if (expectedErrorMessage == null)
        {
            // Valid case: no errors
            Assert.True(_controller.ModelState.IsValid);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Once);
        }
        else
        {
            // Invalid case: error should be present at the expected key
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(expectedKey));
            Assert.Contains(
                _controller.ModelState[expectedKey].Errors,
                e => e.ErrorMessage == expectedErrorMessage);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Never);
        }
    }

    [Theory]
    [InlineData(null, null, null, null)] 
    [InlineData("2025-01-01", "2025-12-31", null, null)] 
    [InlineData("2025-01-01", "2025-01-01", null, null)]
    [InlineData(null, "2025-01-01", null, null)]
    [InlineData("2025-01-01", null, null, null)]
    [InlineData("2025-01-01", "2024-12-31", "DateInput.LessThanOrEqualTo", FormBuilderValidationMessages.MaxDateMustBeLaterThanOrEqualToMin)]
    public async Task Edit_Post_DateInput_Validation_Works_AsExpected(
    string startDateString,
    string endDateString,
    string expectedKey,
    string expectedErrorMessage)
    {
        DateOnly? startDate = string.IsNullOrEmpty(startDateString)
            ? null
            : DateOnly.Parse(startDateString);

        DateOnly? endDate = string.IsNullOrEmpty(endDateString)
            ? null
            : DateOnly.Parse(endDateString);

        var model = _fixture.Build<EditQuestionViewModel>()
            .With(m => m.Type, QuestionType.Date)
            .With(m => m.DateInput, new EditQuestionViewModel.DateInputOptions
            {
                GreaterThanOrEqualTo = startDate,
                LessThanOrEqualTo = endDate
            })
            .With(m => m.AdditionalActions, new EditQuestionViewModel.AdditionalFormActions())
            .With(m => m.Options, new EditQuestionViewModel.Option
            {
                AdditionalFormActions = new EditQuestionViewModel.Option.AdditionalActions(),
                Options = new List<EditQuestionViewModel.Option.OptionItem>()
            })
            .With(m => m.FileUpload, null as EditQuestionViewModel.FileUploadOptions)
            .Create();

        ValidateAndPopulateModelState(model);
        var result = await _controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<EditQuestionViewModel>(viewResult.Model);
        Assert.Same(model, returnedModel);

        if (expectedErrorMessage == null)
        {
            Assert.True(_controller.ModelState.IsValid);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Once);
        }
        else
        {
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(expectedKey));
            Assert.Contains(
                _controller.ModelState[expectedKey].Errors,
                e => e.ErrorMessage == expectedErrorMessage);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Never);
        }
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData(null, 10, null, null)]
    [InlineData(10, null, null, null)]
    [InlineData(1, 10, null, null)] 
    [InlineData(10, 10, null, null)] 
    [InlineData(5, 2, "NumberInput.LessThanOrEqualTo", FormBuilderValidationMessages.MaxNumberMustBeGreaterThanOrEqualToMin)]
    public async Task Edit_Post_NumberInput_Validation_Works_AsExpected(
    int? min,
    int? max,
    string expectedKey,
    string expectedErrorMessage)
    {
        var model = _fixture.Build<EditQuestionViewModel>()
            .With(m => m.Type, QuestionType.Number)
            .With(m => m.NumberInput, new EditQuestionViewModel.NumberInputOptions
            {
                GreaterThanOrEqualTo = min,
                LessThanOrEqualTo = max
            })
            .With(m => m.AdditionalActions, new EditQuestionViewModel.AdditionalFormActions())
            .With(m => m.Options, new EditQuestionViewModel.Option
            {
                AdditionalFormActions = new EditQuestionViewModel.Option.AdditionalActions(),
                Options = new List<EditQuestionViewModel.Option.OptionItem>()
            })
            .With(m => m.FileUpload, null as EditQuestionViewModel.FileUploadOptions)
            .Create();

        ValidateAndPopulateModelState(model);

        var result = await _controller.Edit(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<EditQuestionViewModel>(viewResult.Model);
        Assert.Same(model, returnedModel);

        if (expectedErrorMessage == null)
        {
            Assert.True(_controller.ModelState.IsValid);
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Once);
        }
        else
        {
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(expectedKey));
            Assert.Contains(
                _controller.ModelState[expectedKey].Errors,
                e => e.ErrorMessage == expectedErrorMessage);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateQuestionCommand>(), default), Times.Never);
        }
    }

    #region Validation Setup
    private void SetupValidationServices(FormBuilderSettings settings)
    {
        var services = new ServiceCollection();
        services.AddSingleton(settings);

        var sp = services.BuildServiceProvider();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = sp
            }
        };

        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        _controller.Url = Mock.Of<IUrlHelper>(u =>
        u.Action(It.IsAny<UrlActionContext>()) == "/");
    }

    private void ValidateAndPopulateModelState(object model)
    {
        _controller.ModelState.Clear();
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(
            model,
            _controller.HttpContext?.RequestServices, 
            items: null);

        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);

        foreach (var vr in results)
        {
            var keys = vr.MemberNames?.Any() == true ? vr.MemberNames : new[] { string.Empty };
            foreach (var key in keys)
            {
                _controller.ModelState.AddModelError(key, vr.ErrorMessage);
            }
        }
    }
    #endregion

}
