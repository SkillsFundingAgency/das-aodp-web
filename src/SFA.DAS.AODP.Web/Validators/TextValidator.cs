using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public class TextValidator : IAnswerValidator
    {
        public List<QuestionType> QuestionTypes => [QuestionType.TextArea, QuestionType.Text];

        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
        {
            var required = question.Required;

            var minLength = question.TextInput.MinLength;
            var maxLength = question.TextInput.MaxLength;

            if (required && (answer == null || String.IsNullOrEmpty(answer.TextValue)))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");

            if (minLength is not null && minLength > answer.TextValue?.Length)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be greater than {minLength} characters long.");

            if (maxLength is not null && maxLength < answer.TextValue?.Length)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be less than {maxLength} characters long.");
        }
    }
}