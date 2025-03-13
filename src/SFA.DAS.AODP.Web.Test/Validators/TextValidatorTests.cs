//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class TextValidatorTests : IAnswerValidator
//    {
//        public List<QuestionType> QuestionTypes => [QuestionType.TextArea, QuestionType.Text];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            var minLength = question.TextInput.MinLength;
//            var maxLength = question.TextInput.MaxLength;

//            if (required && (answer == null || String.IsNullOrEmpty(answer.TextValue)))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");

//            var wordCount = answer?.TextValue?.Split().Where(v => !string.IsNullOrEmpty(v)).Count() ?? 0;

//            if (minLength is not null && minLength > wordCount)
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Must have more than {minLength} words.");

//            if (maxLength is not null && maxLength < wordCount)
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Must have less than {maxLength} words.");
//        }
//    }
//}

using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

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