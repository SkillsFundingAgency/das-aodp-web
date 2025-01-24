using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class TextValidator : IQuestionValidator
{
    public bool Required { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }

    public void Validate(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (Required && String.IsNullOrEmpty(answeredQuestion.TextValue)) 
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be blank. ");

        if (MinLength is not null && (String.IsNullOrEmpty(answeredQuestion.TextValue) || MinLength >= answeredQuestion.TextValue.Length))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be greater than {MinLength} characters long. ");

        if (MaxLength is not null && (String.IsNullOrEmpty(answeredQuestion.TextValue) && MaxLength > answeredQuestion.TextValue?.Length))
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must be less than {MaxLength} characters long. ");
    }
}
