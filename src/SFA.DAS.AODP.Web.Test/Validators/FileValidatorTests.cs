//using Microsoft.Extensions.Options;
//using SFA.DAS.AODP.Infrastructure.File;
//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Models.Settings;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class FileValidatorTests : IAnswerValidator
//    {
//        private readonly FormBuilderSettings _formBuilderSettings;
//        private readonly IFileService _fileService;

//        public FileValidatorTests(IOptions<FormBuilderSettings> formBuilderSettings, IFileService fileService)
//        {
//            _formBuilderSettings = formBuilderSettings.Value;
//            _fileService = fileService;
//        }
//        public List<QuestionType> QuestionTypes => [QuestionType.File];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            if (required && (answer == null || answer.FormFiles == null || answer.FormFiles.Count == 0))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide the requested files.");
//            if (answer?.FormFiles == null) return;

//            var maxNumberOfFiles = Math.Min(_formBuilderSettings.MaxUploadNumberOfFiles, question.FileUpload.NumberOfFiles ?? _formBuilderSettings.MaxUploadNumberOfFiles);

//            var existingFilesCount = _fileService.ListBlobs($"{model.ApplicationId}/{question.Id}").Count;

//            if (answer.FormFiles.Count + existingFilesCount > maxNumberOfFiles)
//            {
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload up to {maxNumberOfFiles} file{(maxNumberOfFiles == 1 ? "" : "s")}.");
//            }

//            var maxSize = Math.Min(_formBuilderSettings.MaxUploadFileSize, question.FileUpload.MaxSize ?? _formBuilderSettings.MaxUploadFileSize);
//            long maxSizeBytes = maxSize * 1024 * 1024;
//            foreach (var file in answer.FormFiles)
//            {
//                if (file.Length > maxSizeBytes)
//                {
//                    throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload files up to {maxSize}mb.");
//                }

//                var fileExtension = Path.GetExtension(file.FileName);
//                if (!_formBuilderSettings.UploadFileTypesAllowed.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase))
//                {
//                    throw new QuestionValidationFailedException(question.Id, question.Title, $"You can only upload files of the following types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}");
//                }
//            }

//        }
//    }
//}

using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Infrastructure.File;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

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
            {
                MaxSize = _formBuilderSettings.MaxUploadFileSize
            }
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