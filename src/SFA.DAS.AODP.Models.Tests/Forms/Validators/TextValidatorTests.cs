using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class TextValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.Text;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new TextValidator() { Required = true };
        _answeredQuestion.Object.TextValue = "Hello world";

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.TextValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );

        _answeredQuestion.Object.TextValue = "";
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new TextValidator() { Required = false };
        _answeredQuestion.Object.TextValue = "";

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.TextValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }

    public void Test_MinLength()
    {
        var validator = new TextValidator()
        {
            MinLength = 6,
        };

        _answeredQuestion.Object.TextValue = "Hello world";
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.TextValue = "";
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.MinLength} characters long. "
        );

        _answeredQuestion.Object.TextValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.MinLength} characters long. "
        );
    }

    public void Test_MaxLength()
    {
        var validator = new TextValidator()
        {
            MaxLength = 6,
        };

        _answeredQuestion.Object.TextValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.TextValue = "";
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.TextValue = "Hello world";
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.MinLength} characters long. "
        );
    }
}
