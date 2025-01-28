using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.Validators;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.FormSchema;

public class QuestionSchema
{
    public int Id { get; set; }
    public int PageKey { get; set; }
    public int SectionKey { get; set; }
    public int FormId { get; set; }
    public int FormVersionId { get; set; }
    public int Index { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> MultiChoice { get; set; } = new List<string>();
    public Dictionary<string, RoutingPoint> RoutingPoints { get; set; } = new Dictionary<string, RoutingPoint>();
    public TextValidator? TextValidator { get; set; }
    public IntegerValidator? IntegerValidator { get; set; }
    public DecimalValidator? DecimalValidator { get; set; }
    public DateValidator? DateValidator { get; set; }
    public MultiChoiceValidator? MultiChoiceValidator { get; set; }
    public BooleanValidaor? BooleanValidaor { get; set; }

    public RoutingPoint? ShouldSkip(AnsweredQuestion answeredQuestion)
    {
        if (Type != QuestionType.MultiChoice || answeredQuestion.MultipleChoiceValue is null) return null;
        if (RoutingPoints.TryGetValue(answeredQuestion.MultipleChoiceValue, out var value))
            return value;
        return null;
    }

    public void Validate(AnsweredQuestion answeredQuestion)
    {
        if (answeredQuestion.Type != Type)
        {
            throw new QuestionTypeMismatch(Id, Title, Type, answeredQuestion.Type);
        }

        switch (Type)
        {
            case QuestionType.Text: 
            case QuestionType.TextArea: 
                TextValidator?.Validate(this, answeredQuestion); break;
            case QuestionType.Integer: IntegerValidator?.Validate(this, answeredQuestion); break;
            case QuestionType.Decimal: DecimalValidator?.Validate(this, answeredQuestion); break;
            case QuestionType.Date: DateValidator?.Validate(this, answeredQuestion); break;
            case QuestionType.MultiChoice: MultiChoiceValidator?.Validate(this, answeredQuestion); break;
            case QuestionType.Boolean: BooleanValidaor?.Validate(this, answeredQuestion); break;
        }
    }
}

public enum QuestionType
{
    Text,        // Not null, length min, length max,
    TextArea,        // Not null, length min, length max
    Integer,     // Not null, greater than, less than, equal/not equal to 
    Decimal,     // Not null, greater than, less than, equal/not equal to 
    Date,        // Not null, greater than, less than, equal/not equal to 
    MultiChoice, // Not null
    Radio,
    Boolean      // Not null
}
