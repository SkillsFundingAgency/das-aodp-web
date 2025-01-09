using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class IntegerValidator : IQuestionValidator
{
    public bool Required { get; set; }
    public int? GreaterThan { get; set; }
    public int? LessThan { get; set; }
    public int? EqualTo { get; set; }
    public int? NotEqualTo { get; set; }

    public void Validate(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (Required && answeredQuestion.IntegerValue is null)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be blank. ");

        if (GreaterThan is not null && (answeredQuestion.IntegerValue is null || GreaterThan >= answeredQuestion.IntegerValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {GreaterThan}. ");

        if (LessThan is not null && (answeredQuestion.IntegerValue is not null && LessThan < answeredQuestion.IntegerValue))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be less than {LessThan}. ");

        if (EqualTo is not null && answeredQuestion.IntegerValue != EqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be equal to {EqualTo}. ");

        if (NotEqualTo is not null && answeredQuestion.IntegerValue == NotEqualTo)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be equal to {NotEqualTo}. ");
    }
}
