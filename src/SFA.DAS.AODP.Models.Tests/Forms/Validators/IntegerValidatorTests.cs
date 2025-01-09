using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class IntegerValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.Integer;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new IntegerValidator() { Required = true };
        _answeredQuestion.Object.IntegerValue = 6;


        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new IntegerValidator() { Required = false };
        _answeredQuestion.Object.IntegerValue = 6;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }

    public void Test_GreaterThan()
    {
        var validator = new IntegerValidator()
        {
            GreaterThan = 6,
        };

        _answeredQuestion.Object.IntegerValue = 8;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = 0;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );

        _answeredQuestion.Object.IntegerValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );
    }

    public void Test_LessThan()
    {
        var validator = new IntegerValidator()
        {
            LessThan = 6,
        };

        _answeredQuestion.Object.IntegerValue = 0;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = 8;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be less than {validator.LessThan}. "
        );
    }

    public void Test_EqualTo()
    {
        var validator = new IntegerValidator()
        {
            EqualTo = 6,
        };

        _answeredQuestion.Object.IntegerValue = 6;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = 0;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be equal to {validator.EqualTo}. "
        );

        _answeredQuestion.Object.IntegerValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be equal to {validator.EqualTo}. "
        );
    }

    public void Test_NotEqualTo()
    {
        var validator = new IntegerValidator()
        {
            NotEqualTo = 6,
        };

        _answeredQuestion.Object.IntegerValue = 2;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.IntegerValue = 6;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be equal to {validator.EqualTo}. "
        );
    }
}
