using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.AODP.Models.Exceptions.FormValidation;
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
            return;
            foreach (var question in page.Questions)
            {
                var questionAnswer = viewModel.Questions.First(q => q.Id == question.Id);
                try
                {
                    if (questionAnswer.Type.ToString() != question.Type.ToString())
                        throw new InvalidOperationException("Stored question type does not match the received question type");

                    var validator = _validators.FirstOrDefault(v => v.QuestionTypes.Any(c => c.ToString() == question.Type))
                        ?? throw new NotImplementedException($"Unable to validate the answer for question type {question.Type}");

                    validator.Validate(question, questionAnswer.Answer);
                }
                catch (QuestionValidationFailedException ex)
                {
                    modelState.AddModelError(question.Id.ToString(), ex.Message);
                }


            }
        }
    }
}