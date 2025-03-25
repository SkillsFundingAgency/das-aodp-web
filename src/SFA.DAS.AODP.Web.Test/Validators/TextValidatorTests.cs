//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class TextValidatorTests
{
    private readonly TextValidator _sut = new();

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
            TextValue = null
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please provide a value.", ex.Message);
    }

    [Fact]
    public void Validate_Required_MinimumTextLength_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            TextInput = new()
            {
                MinLength = length
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            TextValue = "Testing"
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Must have more than {length} words.", ex.Message);
    }

    [Fact]
    public void Validate_Required_MaximumTextLength_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int length = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            TextInput = new()
            {
                MaxLength = length
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            TextValue = "Test test test test test test test test test"
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Must have less than {length} words.", ex.Message);
    }
}