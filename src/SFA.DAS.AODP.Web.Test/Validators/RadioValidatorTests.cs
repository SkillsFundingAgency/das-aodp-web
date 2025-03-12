//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Models.Forms;
//using SFA.DAS.AODP.Web.Models.Application;

//namespace SFA.DAS.AODP.Web.Validators
//{
//    public class RadioValidatorTests : IAnswerValidator
//    {


//        public List<QuestionType> QuestionTypes => [QuestionType.Radio];

//        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer? answer, ApplicationPageViewModel model)
//        {
//            var required = question.Required;

//            if (required && (answer == null || String.IsNullOrEmpty(answer.RadioChoiceValue)))
//                throw new QuestionValidationFailedException(question.Id, question.Title, $"Please select a option.");
//        }
//    }
//}