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

            var wordCount = answer?.TextValue?.Split().Where(v => !string.IsNullOrEmpty(v)).Count() ?? 0;

            if (minLength is not null && minLength > wordCount)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"Must have more than {minLength} words.");
            
            if (maxLength is not null && maxLength < wordCount)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"Must have less than {maxLength} words.");
        }
    }
}