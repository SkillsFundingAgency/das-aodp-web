using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class DateValidatorTests
{
    private readonly DateValidator _sut = new();

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
            DateValue = null
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please provide a value.", ex.Message);
    }

    [Fact]
    public void Validate_Required_GreaterThanOrEqualTo_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            DateInput = new()
            {
                GreaterThanOrEqualTo = new(2025, 3, 11)
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            DateValue = new(2025, 3, 10)
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.StartsWith($"The date must be on or after", ex.Message);
    }

    [Fact]
    public void Validate_Required_LessThanOrEqualTo_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            DateInput = new()
            {
                LessThanOrEqualTo = new(2025, 3, 11)
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            DateValue = new(2025, 3, 12)
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.StartsWith($"The date must be on or before", ex.Message);
    }
    [Fact]
    public void Validate_Required_DateInFuture_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            DateInput = new()
            {
                MustBeInFuture = true
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            DateValue = new(2025, 3, 11)
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"The date must be in the future.", ex.Message);
    }

    [Fact]
    public void Validate_Required_DateInPast_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            DateInput = new()
            {
                MustBeInPast = true
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            DateValue = new(9999, 12, 31)
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"The date must be in the past.", ex.Message);
    }
}