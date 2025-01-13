using Moq;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Forms.Validators;

namespace SFA.DAS.AODP.Models.Tests.Forms.FormSchema;

public class QuestionSchemaTests
{

    public QuestionSchema _questionSchema = new QuestionSchema();
    public Mock<AnsweredQuestion> _answeredQuestion = new Mock<AnsweredQuestion>();

    public Mock<TextValidator> TextValidator { get; set; } = new Mock<TextValidator>();
    public Mock<IntegerValidator> IntegerValidator { get; set; } = new Mock<IntegerValidator>();
    public Mock<DecimalValidator> DecimalValidator { get; set; } = new Mock<DecimalValidator>();
    public Mock<DateValidator> DateValidator { get; set; } = new Mock<DateValidator>();
    public Mock<MultiChoiceValidator> MultiChoiceValidator { get; set; } = new Mock<MultiChoiceValidator>();
    public Mock<BooleanValidaor> BooleanValidaor { get; set; } = new Mock<BooleanValidaor>();

    [SetUp]
    public void Setup()
    {
        _questionSchema = new QuestionSchema()
        {
            Title = "HelloWorld",
            Hint = "Hello",
            Type = QuestionType.Text,
            TextValidator = TextValidator.Object,
            IntegerValidator = IntegerValidator.Object,
            DecimalValidator = DecimalValidator.Object,
            DateValidator = DateValidator.Object,
            MultiChoiceValidator = MultiChoiceValidator.Object,
            BooleanValidaor = BooleanValidaor.Object,
        };
    }

    [Test]
    public void Test_TypeMismatch()
    {
        _questionSchema.Type = QuestionType.Text;
        _answeredQuestion.Object.Type = QuestionType.Text;

        Assert.DoesNotThrow(() => _questionSchema.Validate(_answeredQuestion.Object));

        _questionSchema.Type = QuestionType.Text;
        _answeredQuestion.Object.Type = QuestionType.Integer;

        Assert.Throws<QuestionTypeMismatch>(
            () => _questionSchema.Validate(_answeredQuestion.Object),
            $"Expected question of type '{nameof(_questionSchema.Type)}', received '{nameof(_answeredQuestion.Object.Type)}'."
        );
    }

    [Test]
    public void Test_TextValidator()
    {
        _questionSchema.Type = QuestionType.Text;
        _answeredQuestion.Object.Type = QuestionType.Text;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
    }

    [Test]
    public void Test_IntegerValidator()
    {
        _questionSchema.Type = QuestionType.Integer;
        _answeredQuestion.Object.Type = QuestionType.Integer;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
    }

    [Test]
    public void Test_DecimalValidatorr()
    {
        _questionSchema.Type = QuestionType.Decimal;
        _answeredQuestion.Object.Type = QuestionType.Decimal;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
    }

    [Test]
    public void Test_DateValidator()
    {
        _questionSchema.Type = QuestionType.Date;
        _answeredQuestion.Object.Type = QuestionType.Date;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
    }

    [Test]
    public void Test_MultiChoiceValidator()
    {
        _questionSchema.Type = QuestionType.MultiChoice;
        _answeredQuestion.Object.Type = QuestionType.MultiChoice;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
    }

    [Test]
    public void Test_BooleanValidaor()
    {
        _questionSchema.Type = QuestionType.Boolean;
        _answeredQuestion.Object.Type = QuestionType.Boolean;

        _questionSchema.Validate(_answeredQuestion.Object);

        TextValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        IntegerValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DecimalValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        DateValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        MultiChoiceValidator.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Never);
        BooleanValidaor.Verify(v => v.Validate(_questionSchema, _answeredQuestion.Object), Times.Once);
    }
}
