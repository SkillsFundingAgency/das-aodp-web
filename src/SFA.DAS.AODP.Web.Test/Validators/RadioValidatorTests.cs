//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class RadioValidatorTests : IAnswerValidator
//    {


//        public List<QuestionType> QuestionTypes => [QuestionType.Radio];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            if (required && (answer == null || String.IsNullOrEmpty(answer.RadioChoiceValue)))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please select a option.");
//        }
//    }
//}

using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

public class RadioValidatorTests
{
    private readonly RadioValidator _sut = new();

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
            RadioChoiceValue = null
        };

        // Act
        var ex = Assert.Throws<QuestionValidationFailedException>(() => _sut.Validate(question, answer, new()));

        // Assert
        Assert.Equal($"Please select a option.", ex.Message);
    }
}