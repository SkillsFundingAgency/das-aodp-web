using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace SFA.DAS.AODP.Web.Validators.Attributes;
public sealed class QualificationNumberAttribute : ValidationAttribute
{
    public const string DefaultErrorMessage =
        "Enter a qualification number in the format 12345678, 1234567X, 123/4567/8 or 123/4567/X";

    private static readonly Regex QanRegex =
        new(@"^(?:\s*|\d{8}|\d{7}[A-Za-z]|\d{3}\/\d{4}\/(?:\d|[A-Za-z]))$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

    public QualificationNumberAttribute() : base(DefaultErrorMessage) { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;

        if (string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        return QanRegex.IsMatch(s.Trim())
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}
