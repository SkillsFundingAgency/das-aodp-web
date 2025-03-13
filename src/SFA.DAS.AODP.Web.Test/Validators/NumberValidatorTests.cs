//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class NumberValidatorTests : IAnswerValidator
//    {
//        public List<QuestionType> QuestionTypes => [QuestionType.Number];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            var min = question.NumberInput.GreaterThanOrEqualTo;
//            var max = question.NumberInput.LessThanOrEqualTo;
//            var notEqualTo = question.NumberInput.NotEqualTo;

//            if (required && (answer == null || answer.NumberValue == null))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");

//            if (min is not null && (min > answer?.NumberValue))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be greater than or equal to {min}.");

//            if (max is not null && (max < answer?.NumberValue))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be less than or equal to {max}.");

//            if(notEqualTo is not null && notEqualTo == answer?.NumberValue)
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must not be {notEqualTo}.");

//        }
//    }
//}

using Humanizer;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

public class NumberValidatorTests
{
    private readonly NumberValidator _sut = new();

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
    public void Validate_Required_GreaterThanOrEqualTo_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int number = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            NumberInput = new()
            {
                GreaterThanOrEqualTo = number
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            NumberValue = 7
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"The value must be greater than or equal to {number}.", ex.Message);
    }

    [Fact]
    public void Validate_Required_LessThanOrEqualToNumber_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int number = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            NumberInput = new()
            {
                LessThanOrEqualTo = number
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            NumberValue = 9
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"The value must be less than or equal to {number}.", ex.Message);
    }

    [Fact]
    public void Validate_Required_NotEqualTo_ExceptionThrown()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        int number = 8;

        GetApplicationPageByIdQueryResponse.Question question = new()
        {
            Id = questionId,
            Title = "something",
            Required = true,
            NumberInput = new()
            {
                NotEqualTo = number
            }
        };

        ApplicationPageViewModel.Answer answer =
        new()
        {
            NumberValue = 8
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"The value must not be {number}.", ex.Message);
    }
}