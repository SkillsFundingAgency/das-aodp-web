using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class DateValidatorTests
{
    public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    [SetUp]
    public void Setup()
    {
        _questionSchema.Object.Title = "HelloWorld";
        _questionSchema.Object.Id = 5;
        _answeredQuestion.Object.AnsweredStatus = AnsweredStatus.Answered;
        _answeredQuestion.Object.Type = QuestionType.Date;
    }

    [Test]
    public void Test_Required()
    {
        var validator = new DateValidator() { Required = true };
        _answeredQuestion.Object.DateValue = DateTime.UtcNow;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be blank. "
        );
    }

    [Test]
    public void Test_NotRequired()
    {
        var validator = new DateValidator() { Required = false };
        _answeredQuestion.Object.DateValue = DateTime.UtcNow;

        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));
    }

    [Test]
    public void Test_GreaterThan()
    {
        var validator = new DateValidator()
        {
            GreaterThan = new DateTime(2024, 1, 8),
        };

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 9);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 4);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );

        _answeredQuestion.Object.DateValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );
    }

    [Test]
    public void Test_LessThan()
    {
        var validator = new DateValidator()
        {
            LessThan = new DateTime(2024, 1, 8),
        };

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 4);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 9);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be less than {validator.LessThan}. "
        );
    }

    [Test]
    public void Test_EqualTo()
    {
        var validator = new DateValidator()
        {
            EqualTo = new DateTime(2024, 1, 8),
        };

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 8);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 2);
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

    [Test]
    public void Test_NotEqualTo()
    {
        var validator = new DateValidator()
        {
            NotEqualTo = new DateTime(2024, 1, 8),
        };

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 4);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DecimalValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = new DateTime(2024, 1, 8);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must not be equal to {validator.EqualTo}. "
        );
    }

    [Test]
    public void Test_GreaterThanTimeInFuture()
    {
        var validator = new DateValidator()
        {
            GreaterThanTimeInFuture = new DateSpan(1, 1, 1),
        };

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(1).AddMonths(1).AddDays(2);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(1).AddMonths(1);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );

        _answeredQuestion.Object.DateValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );
    }

    [Test]
    public void Test_LessThanTimeInFuture()
    {
        var validator = new DateValidator()
        {
            LessThanTimeInFuture = new DateSpan(1, 1, 1),
        };

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(1).AddMonths(1);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(1).AddMonths(1).AddDays(2);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be less than {validator.LessThan}. "
        );
    }

    [Test]
    public void Test_GreaterThanTimeInPast()
    {
        var validator = new DateValidator()
        {
            GreaterThanTimeInPast = new DateSpan(1, 1, 1),
        };

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(-1).AddMonths(-1);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(-1).AddMonths(-1).AddDays(-2);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );

        _answeredQuestion.Object.DateValue = null;
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be greater than {validator.GreaterThan}. "
        );
    }

    [Test]
    public void Test_LessThanTimeInPast()
    {
        var validator = new DateValidator()
        {
            LessThanTimeInPast = new DateSpan(1, 1, 1),
        };

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(-1).AddMonths(-1).AddDays(-2);
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = null;
        Assert.DoesNotThrow(() => validator.Validate(_questionSchema.Object, _answeredQuestion.Object));

        _answeredQuestion.Object.DateValue = DateTime.Now.AddYears(-1).AddMonths(-1);
        Assert.Throws<QuestionValidationFailed>(
            () => validator.Validate(_questionSchema.Object, _answeredQuestion.Object),
            $"{_questionSchema.Object.Title} must be less than {validator.LessThan}. "
        );
    }
}
