using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class DecimalValidator : IQuestionValidator
{
    public bool Required { get; set; }
    public float? GreaterThan { get; set; }
    public float? LessThan { get; set; }
    public float? EqualTo { get; set; }
    public float? NotEqualTo { get; set; }

    public void Validate(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (Required && answeredQuestion.DecimalValue is null)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be blank. ");

        if (GreaterThan is not null && (answeredQuestion.DecimalValue is null || GreaterThan >= answeredQuestion.DecimalValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {GreaterThan}. ");

        if (LessThan is not null && (answeredQuestion.DecimalValue is not null && LessThan < answeredQuestion.DecimalValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be less than {LessThan}. ");

        if (EqualTo is not null && answeredQuestion.DecimalValue != EqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be equal to {EqualTo}. ");

        if (NotEqualTo is not null && answeredQuestion.DecimalValue == NotEqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be equal to {NotEqualTo}. ");
    }
}
