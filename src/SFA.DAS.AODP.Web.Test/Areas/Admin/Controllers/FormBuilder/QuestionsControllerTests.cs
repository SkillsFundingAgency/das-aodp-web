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
    public async Task Validate_Number_NotEqualIsLessThanOrEqualTo_OnSuccess()
    {
        // Arrange


        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {
                
                LessThanOrEqualTo = 1,
                NotEqualTo = 2,
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
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.DoesNotContain("NumberInput.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Number_NotEqualIsGreaterThanOrEqualTo_OnSuccess()
    {
        // Arrange


        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {

                GreaterThanOrEqualTo = 3,
                NotEqualTo = 2,
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
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.DoesNotContain("NumberInput.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Number_NotEqualToIsLessThanOrEqualTo_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {
                LessThanOrEqualTo = 3,
                NotEqualTo = 2,
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
        Assert.Contains("NumberInput.NotEqualTo.LessThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Number_NotEqualToIsGreaterThanOrEqualTo_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.Number,
            NumberInput = new()
            {
                NotEqualTo = 3,
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
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(1, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("NumberInput.NotEqualTo.GreaterThanOrEqualTo", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_NumOptions_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
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
            Options = new()
            {
                Options = new()
                {
                    new(),
                    new()
                }
            },
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
        Assert.Equal(3, viewResult.ViewData.ModelState.ErrorCount);
        Assert.Contains("Checkbox.MinNumberOfOptions", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("Checkbox.MaxNumberOfOptions", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_MinNumOptions_Negative_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
            Checkbox = new()
            {
                MinNumberOfOptions = 1
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
        Assert.DoesNotContain("Checkbox.MinNumberOfOptions.Negative", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_MinNumOptions_Negative_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new(),
                    new()
                }
            },
            Checkbox = new()
            {
                MinNumberOfOptions = -1
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
        Assert.Contains("Checkbox.MinNumberOfOptions.Negative", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_MaxNumOptions_Negative_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Checkbox = new()
            {
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
        Assert.True(viewResult.ViewData.ModelState.IsValid);
        Assert.Equal(0, viewResult.ViewData.ModelState.ErrorCount);
        Assert.DoesNotContain("Checkbox.MaxNumOptions.Negative", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_MaxNumOptions_Negative_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Checkbox = new()
            {
                MaxNumberOfOptions = -1
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
        Assert.Contains("Checkbox.MaxNumberOfOptions.Negative", viewResult.ViewData.ModelState.Keys);
        Assert.Contains("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_OptionsCount_LessThan_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
            Checkbox = new()
            {
                MinNumberOfOptions = 0
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
        Assert.DoesNotContain("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_OptionsCount_LessThan_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
            Checkbox = new()
            {
                MinNumberOfOptions = 2
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
        Assert.Contains("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_OptionsCount_GreaterThan_OnSuccess()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
            Checkbox = new()
            {
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
        Assert.DoesNotContain("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
    }

    [Fact]
    public async Task Validate_Checkbox_OptionsCount_GreaterThan_OnFailure()
    {
        // Arrange
        var model = new EditQuestionViewModel()
        {
            Type = AODP.Models.Forms.QuestionType.MultiChoice,
            Options = new()
            {
                Options = new()
                {
                    new()
                }
            },
            Checkbox = new()
            {
                MaxNumberOfOptions = 0
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
        Assert.Contains("Options.Options.Count", viewResult.ViewData.ModelState.Keys);
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
}