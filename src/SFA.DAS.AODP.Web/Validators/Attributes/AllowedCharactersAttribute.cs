using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class AllowedCharactersAttribute : ValidationAttribute
{
    private readonly TextCharacterProfile _profile;

    public AllowedCharactersAttribute(TextCharacterProfile profile)
        : base("{0} contains invalid characters.")
    {
        _profile = profile;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
            return ValidationResult.Success;

        s = s.Trim();

        bool isValid = _profile switch
        {
            TextCharacterProfile.PersonName =>
                RegexCache.PersonNameRegex.IsMatch(s),

            TextCharacterProfile.Title =>
                RegexCache.TitleRegex.IsMatch(s),

            TextCharacterProfile.FreeText =>
                IsFreeTextValid(s),

            _ => true
        };

        return isValid
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    private static bool IsFreeTextValid(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        if (text.Contains('<') || text.Contains('>'))
            return false;

        foreach (char c in text)
        {
            if (char.IsControl(c) && c is not ('\r' or '\n' or '\t'))
                return false;
        }

        return true;
    }

    private static class RegexCache
    {
        public static readonly Regex PersonNameRegex =
            new(ValidationPatterns.Text.PersonName, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        public static readonly Regex TitleRegex =
            new(ValidationPatterns.Text.Title, RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    }
}