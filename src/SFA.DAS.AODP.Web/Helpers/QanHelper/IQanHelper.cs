using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Helpers.QanHelper
{
    public interface IQanLookupHelper
    {
        Task<IActionResult> RedirectToRegisterIfQanIsValid(string area, string controller, string qan);
    }
}