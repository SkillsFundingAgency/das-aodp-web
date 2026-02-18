using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class AllowedCharactersAttribute : ValidationAttribute
{
    private readonly TextCharacterProfile _profile;

    public AllowedCharactersAttribute(TextCharacterProfile profile)
    {
        _profile = profile;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var s = value as string;

        if (string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        s = s.Trim();

        var valid = _profile switch
        {
            TextCharacterProfile.QualificationTitle =>
                Regex.IsMatch(s, ValidationPatterns.Text.QualificationTitle),

            TextCharacterProfile.PersonName =>
                Regex.IsMatch(s, ValidationPatterns.Text.PersonName),

            TextCharacterProfile.FreeText =>
                !ContainsControlCharacters(s)
                && !s.Contains('<')
                && !s.Contains('>'),

            _ => true
        };

        return valid
            ? ValidationResult.Success
            : new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} contains invalid characters.");
    }

    private static bool ContainsControlCharacters(string s)
        => s.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
}
