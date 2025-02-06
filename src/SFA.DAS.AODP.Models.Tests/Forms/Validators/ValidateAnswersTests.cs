using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.Validators;

public class ValidateAnswersTests
{
    // public Mock<QuestionSchema> _questionSchema = new Mock<QuestionSchema>();
    public AnsweredQuestion answeredQuestion = new AnsweredQuestion();

    public QuestionSchema questionSchema = new QuestionSchema();

    [SetUp]
    public void Setup()
    {
        answeredQuestion.Id = 1;
        answeredQuestion.QuestionSchemaId = 1;
        answeredQuestion.Index = 1;
        answeredQuestion.AnsweredStatus = AnsweredStatus.Answered;
        questionSchema.Id = 1;
        questionSchema.PageId = 1;
        questionSchema.SectionId = 1;
        questionSchema.FormId = 1;
        questionSchema.Index = 1;
        questionSchema.Title = "Question";
    }

    [Test]
    public void Test_QuestionAgainstTextSchemaNoException()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Text;
        answeredQuestion.TextValue = "This will pass!";

        questionSchema.Type = QuestionType.Text;
        questionSchema.TextValidator = new TextValidator {
            Required = false,
        };

        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }

    [Test]
    public void Test_QuestionAgainstTextSchemaThrowsExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Text;
        answeredQuestion.TextValue = null;
    
        questionSchema.Type = QuestionType.Text;
        questionSchema.TextValidator = new TextValidator {
            Required = true,
        };

        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstTextSchemaThrowsExceptionMinLength()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Text;
        answeredQuestion.TextValue = "This will fail";

        questionSchema.Type = QuestionType.Text;
        questionSchema.TextValidator = new TextValidator {
            Required = false,
            MinLength = 15
        };
        // Act
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Assert
        Assert.AreEqual($"{questionSchema.Title} must be greater than {questionSchema.TextValidator.MinLength} characters long. ", exception.Message);

    }

    [Test]
    public void Test_QuestionAgainstTextSchemaThrowsExceptionMaxLength()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Text;
        answeredQuestion.TextValue = "This will definitely fail";

        questionSchema.Type = QuestionType.Text;
        questionSchema.TextValidator = new TextValidator {
            Required = false,
            MaxLength = 24
        };
        Console.WriteLine(answeredQuestion.TextValue);
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        Console.WriteLine(exception.Message);

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {questionSchema.TextValidator.MaxLength} characters long. ", exception.Message);
    }
    
    [Test]
    public void Test_QuestionAgainstIntegerSchemaNoException()
    {
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = 11;
        // Arrange

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = false,
        };

        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }

    [Test]
    public void Test_QuestionAgainstIntegerSchemaThrowsExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = null;

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = true,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstIntegerSchemaThrowsExceptionGreaterThan()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = 14;

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = false,
            GreaterThan = 15,
        };
        
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be greater than {questionSchema.IntegerValidator.GreaterThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstIntegerSchemaThrowsExceptionLessThan()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = 15;

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = false,
            LessThan = 14,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {questionSchema.IntegerValidator.LessThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstIntegerSchemaThrowsExceptionEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = 20;

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = false,
            EqualTo = 10,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be equal to {questionSchema.IntegerValidator.EqualTo}. ", exception.Message);;
    }

    [Test]
    public void Test_QuestionAgainstIntegerSchemaThrowsExceptionNotEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Integer;
        answeredQuestion.IntegerValue = 15;

        questionSchema.Type = QuestionType.Integer;
        questionSchema.IntegerValidator = new IntegerValidator {
            Required = false,
            NotEqualTo = 15,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be equal to {questionSchema.IntegerValidator.NotEqualTo}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaNoException()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = 11f;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = false,
        };

        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaThrowsExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = null;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = true,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaThrowsExceptionGreaterThan()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = 14f;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = false,
            GreaterThan = 15f
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be greater than {questionSchema.DecimalValidator.GreaterThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaThrowsExceptionLessThan()
    {
        // Arrange
         answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = 15f;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = false,
            LessThan = 14f
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {questionSchema.DecimalValidator.LessThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaThrowsExceptionEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = 20f;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = false,
            EqualTo = 10f
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be equal to {questionSchema.DecimalValidator.EqualTo}. ", exception.Message);;
    }

    [Test]
    public void Test_QuestionAgainstDecimalSchemaThrowsExceptionNotEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Decimal;
        answeredQuestion.DecimalValue = 20f;

        questionSchema.Type = QuestionType.Decimal;
        questionSchema.DecimalValidator = new DecimalValidator {
            Required = false,
            NotEqualTo = 20f
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be equal to {questionSchema.DecimalValidator.NotEqualTo}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaNoException()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 20);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
        };

        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = null;

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = true,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionGreaterThan()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 20);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            GreaterThan = new DateTime(2025, 1, 21)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.NotNull(exception);
        Assert.AreEqual($"{questionSchema.Title} must be greater than {questionSchema.DateValidator.GreaterThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionLessThan()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 21);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            LessThan = new DateTime(2025, 1, 20)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {questionSchema.DateValidator.LessThan}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 20);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            EqualTo = new DateTime(2025, 1, 21)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be equal to {questionSchema.DateValidator.EqualTo}. ", exception.Message);;
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionNotEqualTo()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 20);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            NotEqualTo = new DateTime(2025, 1, 20)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be equal to {questionSchema.DateValidator.NotEqualTo}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionGreaterThanInFuture()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(9999, 12, 31);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            GreaterThanTimeInFuture = new DateTime(2025, 1, 15) - new DateTime(2025, 1, 10)
        };

        DateTime dateTimeNow = DateTime.Now.Date;
        DateTime future = dateTimeNow+questionSchema.DateValidator.GreaterThanTimeInFuture.Value;

        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be greater than {future}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionLessThanInFuture()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 21);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            LessThanTimeInFuture = new DateTime(2025, 1, 15) - new DateTime(2025, 1, 10)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        DateTime dateTimeNow = DateTime.Now.Date;
        DateTime future = dateTimeNow+questionSchema.DateValidator.LessThanTimeInFuture.Value;

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {future}. ", exception.Message);
    }

        [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionGreaterThanInPast()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 20);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            GreaterThanTimeInPast = new DateTime(2025, 1, 20) - new DateTime(2025, 1, 10)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        DateTime dateTimeNow = DateTime.Now.Date;
        DateTime past = dateTimeNow-questionSchema.DateValidator.GreaterThanTimeInPast.Value;

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be greater than {past}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstDateSchemaThrowsExceptionLessThanInPast()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Date;
        answeredQuestion.DateValue = new DateTime(2025, 1, 21);

        questionSchema.Type = QuestionType.Date;
        questionSchema.DateValidator = new DateValidator {
            Required = false,
            LessThanTimeInPast = new DateTime(2025, 1, 15) - new DateTime(2025, 1, 10)
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        DateTime dateTimeNow = DateTime.Now.Date;
        DateTime past = dateTimeNow-questionSchema.DateValidator.LessThanTimeInPast.Value;

        // Act
        Assert.AreEqual($"{questionSchema.Title} must be less than {past}. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstMultipleChoiceSchemaNoException()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.MultiChoice;
        answeredQuestion.MultipleChoiceValue = "Test";

        questionSchema.Type = QuestionType.MultiChoice;
        questionSchema.MultiChoiceValidator = new MultiChoiceValidator {
            Required = false,
            MultiChoice = new List<string> {
                "Test"
            }
        };
        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }

    [Test]
    public void Test_QuestionAgainstMultipleChoiceSchemaExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.MultiChoice;
        answeredQuestion.MultipleChoiceValue = null;

        questionSchema.Type = QuestionType.MultiChoice;
        questionSchema.MultiChoiceValidator = new MultiChoiceValidator {
            Required = true,

        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstMultipleChoiceSchemaExceptionNoValidOption()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.MultiChoice;
        answeredQuestion.MultipleChoiceValue = "Test 4";

        questionSchema.Type = QuestionType.MultiChoice;
        questionSchema.MultiChoiceValidator = new MultiChoiceValidator {
            Required = false,
            MultiChoice = new List<string> {
                "Test 1",
                "Test 2",
                "Test 3"
            }
        };
        // Assert
        var exception = Assert.Throws<MultipleChoiceOptionException>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"Option passed to '{questionSchema.Title}' - '{answeredQuestion.MultipleChoiceValue}' is not a valid option. ", exception.Message);
    }

    [Test]
    public void Test_QuestionAgainstBooleanSchemaNoException()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Boolean;
        answeredQuestion.BooleanValue = true;

        questionSchema.Type = QuestionType.Boolean;
        questionSchema.BooleanValidator = new BooleanValidator {
            Required = false,
        };
        // Act & Assert
        Assert.DoesNotThrow(() => questionSchema.Validate(answeredQuestion));
    }
    [Test]
    public void Test_QuestionAgainstBooleanSchemaExceptionRequired()
    {
        // Arrange
        answeredQuestion.Type = QuestionType.Boolean;
        answeredQuestion.BooleanValue = null;

        questionSchema.Type = QuestionType.Boolean;
        questionSchema.BooleanValidator = new BooleanValidator {
            Required = true,
        };
        // Assert
        var exception = Assert.Throws<QuestionValidationFailed>(
            () => questionSchema.Validate(answeredQuestion));

        // Act
        Assert.AreEqual($"{questionSchema.Title} must not be blank. ", exception.Message);
    }
}
