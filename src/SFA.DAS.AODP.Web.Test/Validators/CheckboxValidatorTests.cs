using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;
public class CheckboxValidatorTests
{
    private readonly CheckboxValidator _sut = new();

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
        Assert.Equal($"Please select at least 1 option.", ex.Message);
    }

    [Fact]
    public void Validate_Required_NotEnoughOptionsSelected_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int numOptions = 2;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            Checkbox = new()
            {
                MinNumberOfOptions = numOptions
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            MultipleChoiceValues = ["a"]
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please select at least {numOptions} options.", ex.Message);
    }

    [Fact]
    public void Validate_Required_TooManyOptionsSelected_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int numOptions = 2;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            Checkbox = new()
            {
                MaxNumberOfOptions = numOptions
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            MultipleChoiceValues = ["a", "b", "c"]
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please only select up to {numOptions} options.", ex.Message);
    }
}

