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
    private readonly FormBuilderSettings _formBuilderSettings;
    private readonly FileValidator _sut;
    private readonly Mock<IFileService> _fileService = new();

    public FileValidatorTests()
    {
        _formBuilderSettings = new FormBuilderSettings
        {
            MaxUploadNumberOfFiles = 1,
            MaxUploadFileSize = 100, // MB
            UploadFileTypesAllowed = new() { ".pdf", ".png", ".jpg" }
        };

        _sut = new FileValidator(
            Options.Create(_formBuilderSettings),
            _fileService.Object);
    }

    private static ApplicationPageViewModel BuildModel(Guid questionId, int uploadedCount)
    {
        return new ApplicationPageViewModel
        {
            Questions = new()
            {
                new ApplicationPageViewModel.Question
                {
                    Id = questionId,
                    UploadedFiles = Enumerable
                        .Range(0, uploadedCount)
                        .Select(_ => new ApplicationPageViewModel.UploadedFile())
                        .ToList()
                }
            }
        };
    }

    [Fact]
    public void Validate_Required_NoFilesProvided_Throws()
    {
        var questionId = Guid.NewGuid();

        var question = new GetApplicationPageByIdQueryResponse.Question
        {
            Id = questionId,
            Title = "something",
            Required = true
        };

        var answer = new ApplicationPageViewModel.Answer
        {
            FormFiles = null
        };

        var model = BuildModel(questionId, uploadedCount: 0);

        var ex = Assert.Throws<QuestionValidationFailedException>(
            () => _sut.Validate(question, answer, model));

        Assert.Equal("Please provide the requested files.", ex.Message);
    }

    [Fact]
    public void Validate_Required_ExistingFiles_NoNewFiles_Passes()
    {
        var questionId = Guid.NewGuid();

        var question = new GetApplicationPageByIdQueryResponse.Question
        {
            Id = questionId,
            Title = "something",
            Required = true
        };

        var answer = new ApplicationPageViewModel.Answer
        {
            FormFiles = null
        };

        var model = BuildModel(questionId, uploadedCount: 1);

        // Act / Assert (no exception)
        _sut.Validate(question, answer, model);
    }

    [Fact]
    public void Validate_MaxNumberOfFiles_Exceeded_Throws()
    {
        var questionId = Guid.NewGuid();

        // MaxUploadNumberOfFiles = 1 (from baseline)

        var question = new GetApplicationPageByIdQueryResponse.Question
        {
            Id = questionId,
            Title = "something",
            Required = true
        };

        var file1 = new Mock<IFormFile>();
        var file2 = new Mock<IFormFile>();

        var answer = new ApplicationPageViewModel.Answer
        {
            FormFiles = new() { file1.Object, file2.Object }
        };

        var model = BuildModel(questionId, uploadedCount: 0);

        var ex = Assert.Throws<QuestionValidationFailedException>(
            () => _sut.Validate(question, answer, model));

        Assert.Equal("You can only upload up to 1 file.", ex.Message);
    }

    [Fact]
    public void Validate_FileTooLarge_Throws()
    {
        var questionId = Guid.NewGuid();

        // MaxUploadFileSize = 100MB (from baseline)

        var question = new GetApplicationPageByIdQueryResponse.Question
        {
            Id = questionId,
            Title = "something",
            Required = true
        };

        var file = new Mock<IFormFile>();
        file.SetupGet(f => f.Length)
            .Returns(101L * 1024 * 1024); // 101MB
        file.SetupGet(f => f.FileName)
            .Returns("test.pdf");

        var answer = new ApplicationPageViewModel.Answer
        {
            FormFiles = new() { file.Object }
        };

        var model = BuildModel(questionId, uploadedCount: 0);

        var ex = Assert.Throws<QuestionValidationFailedException>(
            () => _sut.Validate(question, answer, model));

        Assert.Equal("You can only upload files up to 100mb.", ex.Message);
    }

    [Fact]
    public void Validate_InvalidFileExtension_Throws()
    {
        var questionId = Guid.NewGuid();

        // Allowed extensions: .pdf .png .jpg (baseline)

        var question = new GetApplicationPageByIdQueryResponse.Question
        {
            Id = questionId,
            Title = "something",
            Required = true
        };

        var file = new Mock<IFormFile>();
        file.SetupGet(f => f.FileName)
            .Returns("malware.exe");
        file.SetupGet(f => f.Length)
            .Returns(1024); // small enough to pass size check

        var answer = new ApplicationPageViewModel.Answer
        {
            FormFiles = new() { file.Object }
        };

        var model = BuildModel(questionId, uploadedCount: 0);

        var ex = Assert.Throws<QuestionValidationFailedException>(
            () => _sut.Validate(question, answer, model));

        Assert.Equal(
            "You can only upload files of the following types: .pdf,.png,.jpg",
            ex.Message);
    }
}