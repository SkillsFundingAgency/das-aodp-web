using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public class NumberValidator : IAnswerValidator
    {
        public List<QuestionType> QuestionTypes => [QuestionType.Number];

        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer)
        {
            var required = question.Required;

            var min = question.NumberInput.GreaterThanOrEqualTo;
            var max = question.NumberInput.LessThanOrEqualTo;
            var notEqualTo = question.NumberInput.NotEqualTo;

            if (required && (answer == null || answer.NumberValue == null))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");

            if (min is not null && (min > answer?.NumberValue))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be greater than or equal to {min}.");

            if (max is not null && (max < answer?.NumberValue))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must be less than or equal to {max}.");
            
            if(notEqualTo is not null && notEqualTo == answer?.NumberValue)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The value must not be {notEqualTo}.");

        }
    }
}