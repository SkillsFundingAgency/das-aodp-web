using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public class DateValidator : IAnswerValidator
    {
        public List<QuestionType> QuestionTypes => [QuestionType.Date];

        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
        {
            var required = question.Required;

            var min = question.DateInput.GreaterThanOrEqualTo;
            var max = question.DateInput.LessThanOrEqualTo;
            var future = question.DateInput.MustBeInFuture ?? false;
            var past = question.DateInput.MustBeInPast ?? false;

            if (required && (answer == null || answer.DateValue == null))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please provide a value.");
            if (answer.DateValue == null) return;

            if (min is not null && min > answer!.DateValue)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be on or after {min.Value:dd/MM/yyyy}.");

            if (max is not null && max < answer!.DateValue)
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be on or before {max.Value:dd/MM/yyyy}.");

            if (future && answer!.DateValue!.Value <= DateOnly.FromDateTime(DateTime.Now))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be in the future.");

            if (past && answer!.DateValue!.Value >= DateOnly.FromDateTime(DateTime.Now))
                throw new QuestionValidationFailedException(question.Id, question.Title, $"The date must be in the past.");

        }
    }
}