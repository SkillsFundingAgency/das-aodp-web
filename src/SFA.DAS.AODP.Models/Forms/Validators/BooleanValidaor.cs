using SFA.DAS.AODP.Models.Forms.Application;
using SFA.DAS.AODP.Models.Forms.FormSchema;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;

namespace SFA.DAS.AODP.Models.Forms.Validators;

public class BooleanValidaor : IQuestionValidator
{
    public bool Required { get; set; }

    public void Validate(QuestionSchema schema, AnsweredQuestion answeredQuestion)
    {
        if (Required && answeredQuestion.BooleanValue is null)
            throw new QuestionValidationFailed(schema.Id, schema.Title, $"{schema.Title} must not be blank. ");
    }
}
