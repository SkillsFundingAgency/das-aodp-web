using System.ComponentModel.DataAnnotations;
using SFA.DAS.AODP.Web.Validators.Attributes;

namespace SFA.DAS.AODP.Web.Test.Validators.Attributes;

public class QualificationNumberAttributeTests
{
    private readonly QualificationNumberAttribute _sut = new();

    private static ValidationResult? Validate(object? value, ValidationAttribute attribute)
    {
        var context = new ValidationContext(new object());
        return attribute.GetValidationResult(value, context);
    }

    [Fact]
    public void Validate_Null_Should_Pass()
    {
        // Arrange
        object? value = null;

        // Act
        var result = Validate(value, _sut);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Validate_EmptyOrWhitespace_Should_Pass(string value)
    {
        // Act
        var result = Validate(value, _sut);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("12345678")]     // 8 digits
    [InlineData("1234567X")]     // 7 digits + letter
    [InlineData("1234567a")]     // lower-case letter allowed
    [InlineData("123/4567/8")]   // slash format ending in digit
    [InlineData("123/4567/X")]   // slash format ending in letter
    [InlineData("123/4567/x")]   // lower-case letter allowed
    public void Validate_ValidQan_Should_Pass(string value)
    {
        // Act
        var result = Validate(value, _sut);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(" 12345678 ")]
    [InlineData("\t1234567X\r\n")]
    [InlineData(" 123/4567/X ")]
    public void Validate_ValidQan_WithSurroundingWhitespace_Should_Pass(string value)
    {
        // Act
        var result = Validate(value, _sut);

        // Assert
        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("1234567")]        // too short
    [InlineData("123456789")]      // too long
    [InlineData("1234567XX")]      // too long
    [InlineData("1234567-")]       // invalid last char
    [InlineData("123/4567")]       // missing last segment
    [InlineData("12/4567/8")]      // first segment not 3 digits
    [InlineData("123/456/8")]      // middle segment not 4 digits
    [InlineData("123/4567/88")]    // last segment too long
    [InlineData("abc/4567/8")]     // invalid first segment
    [InlineData("123/abcd/8")]     // invalid middle segment
    [InlineData("1234567*")]       // invalid symbol
    public void Validate_InvalidQan_Should_Have_Error(string value)
    {
        // Act
        var result = Validate(value, _sut);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal(QualificationNumberAttribute.DefaultErrorMessage, result!.ErrorMessage);
        });
    }

    [Fact]
    public void Validate_InvalidQan_WithCustomErrorMessage_Should_Use_CustomMessage()
    {
        // Arrange
        var attribute = new QualificationNumberAttribute
        {
            ErrorMessage = "Custom error"
        };

        // Act
        var result = Validate("not-a-qan", attribute);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Custom error", result!.ErrorMessage);
        });
    }
}
