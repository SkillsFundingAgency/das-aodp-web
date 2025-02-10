using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Validators
{
    public interface IApplicationAnswersValidator
    {
        void ValidateApplicationPageAnswers(ModelStateDictionary modelState, GetApplicationPageByIdQueryResponse page, ApplicationPageViewModel viewModel);
    }
}