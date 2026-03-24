using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class AllowedCharactersAttribute : ValidationAttribute
{
    private readonly TextCharacterProfile _profile;

    private static readonly Regex TitleRegex =
        new(ValidationPatterns.Text.QualificationTitle,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

    private static readonly Regex PersonNameRegex =
        new(ValidationPatterns.Text.PersonName,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(100));

    public AllowedCharactersAttribute(TextCharacterProfile profile)
        :base("{0} contains invalid characters.") 
    {
        _profile = profile;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name);
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
                TitleRegex.IsMatch(s),

            TextCharacterProfile.PersonName =>
                PersonNameRegex.IsMatch(s),

            TextCharacterProfile.FreeText =>
                !ContainsControlCharacters(s)
                && !s.Contains('<')
                && !s.Contains('>'),

            _ => true
        };

        return valid
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    private static bool ContainsControlCharacters(string s)
        => s.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
}
