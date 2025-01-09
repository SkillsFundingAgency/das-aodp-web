using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class MultiChoiceValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _questionSchema.Object.MultiChoice = new List<string>() { "Hello", "World" };
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.MultiChoice;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new MultiChoiceValidator() { Required = true };

        _answeredQuestion.Object.MultipleChoiceValue = "Hello";
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.MultipleChoiceValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new MultiChoiceValidator() { Required = false };

        _answeredQuestion.Object.MultipleChoiceValue = "Hello";
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.MultipleChoiceValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }

    [Test]
    public void Test_IsValidOption()
    {
        var validator = new MultiChoiceValidator() { Required = false };

        _answeredQuestion.Object.MultipleChoiceValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.MultipleChoiceValue = "Hello";
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.MultipleChoiceValue = "HelloWorld";
        Assert.Throws<MultipleChoiceOptionException>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"Option passed to '{_questionSchema.Object.Title}' - '{_answeredQuestion.Object.MultipleChoiceValue}' is not a valid option. "
        );

    }
}
