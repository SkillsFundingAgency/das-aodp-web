using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class FluentValidationModelStateExtensions
    {
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string? prefix = null)
        {
            foreach (var error in result.Errors)
            {
                var key = string.IsNullOrEmpty(prefix) ? error.PropertyName : $"{prefix}.{error.PropertyName}";
                modelState.AddModelError(key, error.ErrorMessage);
            }
        }
    }
}
