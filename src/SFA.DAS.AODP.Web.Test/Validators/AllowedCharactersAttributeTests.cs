using System.ComponentModel.DataAnnotations;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;
using Xunit;

namespace SFA.DAS.AODP.Web.Tests.Validators.Attributes;

public class AllowedCharactersAttributeTests
{
    private const string DefaultFieldName = "Field";
    private const string NameField = "Name";
    private const string DescriptionField = "Description";

    private const string ValidPersonName = "John Smith";
    private const string InvalidPersonName = "John<>";

    private const string ValidFreeText = "This is normal text.";
    private const string FreeTextWithScript = "Hello <script>";
    private const string FreeTextWithControlCharacter = "Hello\u0001World";

    private const string ValidQualificationTitle = "Level 3 Diploma in Engineering";

    private static ValidationContext CreateContext(string displayName = DefaultFieldName)
    {
        return new ValidationContext(new object())
        {
            DisplayName = displayName
        };
    }

    [Fact]
    public void ReturnsSuccess_WhenValueIsNull()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.PersonName);

        var result = attribute.GetValidationResult(null, CreateContext());

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void ReturnsSuccess_WhenValueIsWhitespace()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.PersonName);

        var result = attribute.GetValidationResult("   ", CreateContext());

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void PersonNameProfile_AllowsValidName()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.PersonName);

        var result = attribute.GetValidationResult(ValidPersonName, CreateContext());

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void PersonNameProfile_ReturnsError_WhenInvalidCharacters()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.PersonName);

        var result = attribute.GetValidationResult(InvalidPersonName, CreateContext(NameField));

        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal($"{NameField} contains invalid characters.", result!.ErrorMessage);
    }

    [Fact]
    public void FreeText_ReturnsError_WhenContainsAngleBrackets()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.FreeText);

        var result = attribute.GetValidationResult(FreeTextWithScript, CreateContext(DescriptionField));

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void FreeText_AllowsNormalText()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.FreeText);

        var result = attribute.GetValidationResult(ValidFreeText, CreateContext());

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void FreeText_ReturnsError_WhenContainsControlCharacters()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.FreeText);

        var result = attribute.GetValidationResult(FreeTextWithControlCharacter, CreateContext());

        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void QualificationTitle_ValidTitle_ReturnsSuccess()
    {
        var attribute = new AllowedCharactersAttribute(TextCharacterProfile.QualificationTitle);

        var result = attribute.GetValidationResult(ValidQualificationTitle, CreateContext());

        Assert.Equal(ValidationResult.Success, result);
    }
}