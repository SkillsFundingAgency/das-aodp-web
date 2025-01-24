using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.Validators;
using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class BooleanValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.Boolean;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new BooleanValidaor() { Required = true };
        _answeredQuestion.Object.BooleanValue = true;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.BooleanValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new BooleanValidaor() { Required = false };
        _answeredQuestion.Object.BooleanValue = true;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.BooleanValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }
}
