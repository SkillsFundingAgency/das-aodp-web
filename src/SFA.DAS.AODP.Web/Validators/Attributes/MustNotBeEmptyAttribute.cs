using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Validators.Attributes;

/// <summary>
/// Defines a validation attribute that validates that a collection is not empty.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MustNotBeEmptyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IList list)
        {
            var valid = list.Count > 0;
            if (valid)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage,
                [validationContext?.MemberName!]);
        }

        return ValidationResult.Success;
    }
}