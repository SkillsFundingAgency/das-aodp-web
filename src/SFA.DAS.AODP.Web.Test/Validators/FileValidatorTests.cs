using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class FileValidatorTests
{
    private readonly FormBuilderSettings _formBuilderSettings = new FormBuilderSettings();
    private readonly FileValidator _sut;
    private readonly Mock<IFileService> _fileService = new();

    public FileValidatorTests()
    {
        _sut = new FileValidator(Options.Create(_formBuilderSettings), _fileService.Object);
    }

    [Fact]
    public void Validate_Required_NoAnswerProvided_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _fileService.Setup(s => s.ListBlobs(It.IsAny<string>()))
            .Returns([]);

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            FormFiles = null
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please provide the requested files.", ex.Message);
    }

    [Fact]
    public void Validate_Required_ExisitingFiles_NoAnswerProvided_NoExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _fileService.Setup(s => s.ListBlobs(It.IsAny<string>()))
            .Returns([new()]);

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            FormFiles = null
        };

        // Act
        _sut.Validate(question, answer, new());
    }

    [Fact]
    public void Validate_Required_MaxNumberOfFiles_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _formBuilderSettings.MaxUploadNumberOfFiles = 1;

        _fileService.Setup(s => s.ListBlobs(It.IsAny<string>()))
            .Returns([]);

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            FileUpload = new()
            {
                NumberOfFiles = _formBuilderSettings.MaxUploadNumberOfFiles
            }
        };

        Mock<IFormFile> _formFile1 = new();
        Mock<IFormFile> _formFile2 = new();

        ApplicationPageViewModel.Answer answer =
        new()
        {
          FormFiles = new()
          {
              _formFile1.Object,
              _formFile2.Object
          }
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"You can only upload up to {_formBuilderSettings.MaxUploadNumberOfFiles} file{(_formBuilderSettings.MaxUploadNumberOfFiles == 1 ? "" : "s")}.", ex.Message);
    }

    [Fact]
    public void Validate_Required_MaxFileSize_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _formBuilderSettings.MaxUploadNumberOfFiles = 2;

        _formBuilderSettings.MaxUploadFileSize = 1;

        _fileService.Setup(s => s.ListBlobs(It.IsAny<string>()))
            .Returns([]);

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            FileUpload = new()
        };

        Mock<IFormFile> _formFile = new()
        {

        };

        _formFile.SetupGet(s => s.Length)
            .Returns(2*1024*1024);

        ApplicationPageViewModel.Answer answer =
        new()
        {
            FormFiles = new()
            {
                _formFile.Object
            }
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"You can only upload files up to {_formBuilderSettings.MaxUploadFileSize}mb.", ex.Message);
    }

    [Fact]
    public void Validate_Required_AllowedFileExtensions_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _formBuilderSettings.UploadFileTypesAllowed = ["pdf"];
        _formBuilderSettings.MaxUploadNumberOfFiles = 2;

        _fileService.Setup(s => s.ListBlobs(It.IsAny<string>()))
            .Returns(
            [
                new(){
                    FileName = "something.pdf"
                }
            ]
        );

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            FileUpload = new()
            {
                FileNamePrefix = _formBuilderSettings.UploadFileTypesAllowed[0]
            }
        };

        Mock<IFormFile> _formFile = new()
        {

        };

        _formFile.SetupGet(s => s.FileName)
            .Returns("something.png");

        ApplicationPageViewModel.Answer answer =
        new()
        {
            FormFiles = new()
            {
                _formFile.Object
            }
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"You can only upload files of the following types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}", ex.Message);
    }
}