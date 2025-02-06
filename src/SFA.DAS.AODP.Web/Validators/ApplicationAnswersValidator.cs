using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public class ApplicationAnswersValidator : IApplicationAnswersValidator
    {
        private readonly IEnumerable<IAnswerValidator> _validators;

        public ApplicationAnswersValidator(IEnumerable<IAnswerValidator> validators)
        {
            _validators = validators;
        }

        public void ValidateApplicationPageAnswers(ModelStateDictionary modelState, GetApplicationPageByIdQueryResponse page, ApplicationPageViewModel viewModel)
        {
            foreach (var question in page.Questions)
            {
                var questionAnswer = viewModel.Questions.First(q => q.Id == question.Id);
                try
                {
                    if (questionAnswer.Type.ToString() != question.Type.ToString())
                        throw new InvalidOperationException("Stored question type does not match the received question type");

                    var validator = _validators.FirstOrDefault(v => v.QuestionType.ToString() == question.Type)
                        ?? throw new NotImplementedException($"Unable to validate the answer for question type {question.Type}");

                    validator.Validate(question, questionAnswer.Answer);
                }
                catch (QuestionValidationFailed ex)
                {
                    modelState.AddModelError(question.Id.ToString(), ex.Message);
                }


            }
        }
    }

    public interface IAnswerValidator
    {
        public QuestionType QuestionType { get; }
        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer);
    }

    public class TextValidator : IAnswerValidator
    {
        public QuestionType QuestionType => QuestionType.Text;

        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer)
        {
            var required = question.Required;

            var minLength = question.TextInput.MinLength;
            var maxLength = question.TextInput.MaxLength;

            if (required && (answer == null || String.IsNullOrEmpty(answer.TextValue)))
                throw new QuestionValidationFailed(question.Id, question.Title, $"Please provide a value.");

            if (minLength is not null && (String.IsNullOrEmpty(answer!.TextValue) || minLength >= answer.TextValue.Length))
                throw new QuestionValidationFailed(question.Id, question.Title, $"The value must be greater than {minLength} characters long.");

            if (maxLength is not null && (String.IsNullOrEmpty(answer!.TextValue) && maxLength > answer.TextValue?.Length))
                throw new QuestionValidationFailed(question.Id, question.Title, $"The value must be less than {maxLength} characters long.");
        }
    }

    public class RadioValidator : IAnswerValidator
    {
        public QuestionType QuestionType => QuestionType.Radio;

        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer)
        {
            var required = question.Required;

            if (required && (answer == null || String.IsNullOrEmpty(answer.RadioChoiceValue)))
                throw new QuestionValidationFailed(question.Id, question.Title, $"Please select a option.");
        }
    }
}