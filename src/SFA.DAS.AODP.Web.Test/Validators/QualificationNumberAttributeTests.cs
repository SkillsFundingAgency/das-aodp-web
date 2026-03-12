using System.ComponentModel.DataAnnotations;
using SFA.DAS.AODP.Web.Validators.Attributes;
using Xunit;

namespace SFA.DAS.AODP.Web.Test.Validators.Attributes;

public class QualificationNumberAttributeTests
{
    private readonly QualificationNumberAttribute _sut = new();

    private const string ValidQanDigits = "12345678";
    private const string ValidQanLetter = "1234567X";
    private const string ValidQanLowercaseLetter = "1234567a";

    private const string ValidQanSlashDigit = "123/4567/8";
    private const string ValidQanSlashLetter = "123/4567/X";
    private const string ValidQanSlashLowerLetter = "123/4567/x";

    private const string ValidQanWhitespace1 = " 12345678 ";
    private const string ValidQanWhitespace2 = "\t1234567X\r\n";
    private const string ValidQanWhitespace3 = " 123/4567/X ";

    private const string InvalidQan = "not-a-qan";
    private const string CustomErrorMessage = "Custom error";

    private static ValidationResult? Validate(object? value, ValidationAttribute attribute)
    {
        var context = new ValidationContext(new object());
        return attribute.GetValidationResult(value, context);
    }

    [Fact]
    public void Validate_Null_Should_Pass()
    {
        var result = Validate(null, _sut);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Validate_EmptyOrWhitespace_Should_Pass(string value)
    {
        var result = Validate(value, _sut);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(ValidQanDigits)]
    [InlineData(ValidQanLetter)]
    [InlineData(ValidQanLowercaseLetter)]
    [InlineData(ValidQanSlashDigit)]
    [InlineData(ValidQanSlashLetter)]
    [InlineData(ValidQanSlashLowerLetter)]
    public void Validate_ValidQan_Should_Pass(string value)
    {
        var result = Validate(value, _sut);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(ValidQanWhitespace1)]
    [InlineData(ValidQanWhitespace2)]
    [InlineData(ValidQanWhitespace3)]
    public void Validate_ValidQan_WithSurroundingWhitespace_Should_Pass(string value)
    {
        var result = Validate(value, _sut);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("1234567XX")]
    [InlineData("1234567-")]
    [InlineData("123/4567")]
    [InlineData("12/4567/8")]
    [InlineData("123/456/8")]
    [InlineData("123/4567/88")]
    [InlineData("abc/4567/8")]
    [InlineData("123/abcd/8")]
    [InlineData("1234567*")]
    public void Validate_InvalidQan_Should_Have_Error(string value)
    {
        var result = Validate(value, _sut);

        Assert.Multiple(() =>
        {
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal(QualificationNumberAttribute.DefaultErrorMessage, result!.ErrorMessage);
        });
    }

    [Fact]
    public void Validate_InvalidQan_WithCustomErrorMessage_Should_Use_CustomMessage()
    {
        var attribute = new QualificationNumberAttribute
        {
            ErrorMessage = CustomErrorMessage
        };

        var result = Validate(InvalidQan, attribute);

        Assert.Multiple(() =>
        {
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal(CustomErrorMessage, result!.ErrorMessage);
        });
    }
}