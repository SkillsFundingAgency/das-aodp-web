//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class DateValidatorTests : IAnswerValidator
//    {
//        public List<QuestionType> QuestionTypes => [QuestionType.Date];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            var min = question.DateInput.GreaterThanOrEqualTo;
//            var max = question.DateInput.LessThanOrEqualTo;
//            var future = question.DateInput.MustBeInFuture ?? false;
//            var past = question.DateInput.MustBeInPast ?? false;

//            if (required && (answer == null || answer.DateValue == null))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");
//            if (answer.DateValue == null) return;

//            if (min is not null && min > answer!.DateValue)
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be on or after {min.Value:dd/MM/yyyy}.");

//            if (max is not null && max < answer!.DateValue)
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be on or before {max.Value:dd/MM/yyyy}.");

//            if (future && answer!.DateValue!.Value <= DateOnly.FromDateTime(DateTime.Now))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be in the future.");

//            if (past && answer!.DateValue!.Value >= DateOnly.FromDateTime(DateTime.Now))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be in the past.");

//        }
//    }
//}

using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

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