using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class DecimalValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.Decimal;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new DecimalValidator() { Required = true };
        _answeredQuestion.Object.DecimalValue = 1.5f;


        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new DecimalValidator() { Required = false };
        _answeredQuestion.Object.DecimalValue = 1.5f;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }

    public void Test_GreaterThan()
    {
        var validator = new DecimalValidator()
        {
            GreaterThan = 6,
        };

        _answeredQuestion.Object.DecimalValue = 8;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = 0;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );

        _answeredQuestion.Object.DecimalValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );
    }

    public void Test_LessThan()
    {
        var validator = new DecimalValidator()
        {
            LessThan = 6,
        };

        _answeredQuestion.Object.DecimalValue = 0;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = 8;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be less than {validator.LessThan}. "
        );
    }

    public void Test_EqualTo()
    {
        var validator = new DecimalValidator()
        {
            EqualTo = 6,
        };

        _answeredQuestion.Object.DecimalValue = 6;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = 0;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be equal to {validator.EqualTo}. "
        );

        _answeredQuestion.Object.DecimalValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be equal to {validator.EqualTo}. "
        );
    }

    public void Test_NotEqualTo()
    {
        var validator = new DecimalValidator()
        {
            NotEqualTo = 6,
        };

        _answeredQuestion.Object.DecimalValue = 2;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = 6;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be equal to {validator.EqualTo}. "
        );
    }
}
