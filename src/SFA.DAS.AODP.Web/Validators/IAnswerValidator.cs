using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public interface IAnswerValidator
    {
        public List<QuestionType> QuestionTypes { get; }
        public void Validate(GetApplicationPageByIdQueryResponse.Question question, ApplicationPageViewModel.Answer answer, ApplicationPageViewModel model);
    }
}