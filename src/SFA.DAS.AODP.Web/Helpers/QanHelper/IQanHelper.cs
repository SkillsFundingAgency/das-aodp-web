using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Helpers.QanHelper
{
    public interface IQanLookupHelper
    {
        Task<IActionResult> RedirectToRegisterIfQanIsValid(string areaName, string controllerName, string qan);
    }
}