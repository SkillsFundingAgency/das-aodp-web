using SFA.DAS.AODP.Web.Models.OutputFile;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class OutputFileFormValidatorTests
{
    private readonly OutputFileViewModelValidator _sut = new();

    [Fact]
    public void Validate_DateChoice_None_Should_Have_Error()
    {
        // Arrange
        var model = new OutputFileViewModel { DateChoice = PublicationDateMode.None };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Choose a publication date option.");
        });
    }

    [Fact]
    public void Validate_Manual_MissingParts_Should_Have_GroupedErrors()
    {
        // Arrange
        var model = new OutputFileViewModel
        {
            DateChoice = PublicationDateMode.Manual,
            Day = null,
            Month = null,
            Year = null
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsValid);
            Assert.Equal(4, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Enter a publication date");
        });
    }

    [Fact]
    public void Validate_Manual_InvalidDate_Should_Error()
    {
        // Arrange
        var model = new OutputFileViewModel
        {
            DateChoice = PublicationDateMode.Manual,
            Day = 31,
            Month = 2,
            Year = 2025
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Publication date must be a real date");
        });
    }

    [Fact]
    public void Validate_Manual_PastDate_Should_Error()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var model = new OutputFileViewModel
        {
            DateChoice = PublicationDateMode.Manual,
            Day = yesterday.Day,
            Month = yesterday.Month,
            Year = yesterday.Year
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Publication date must be today or in the future");
        });
    }

    [Fact]
    public void Validate_TodayMode_Should_Pass()
    {
        // Arrange
        var model = new OutputFileViewModel { DateChoice = PublicationDateMode.Today };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        });
    }

    [Fact]
    public void Validate_Manual_TodayOrFuture_Should_Pass()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        var model = new OutputFileViewModel
        {
            DateChoice = PublicationDateMode.Manual,
            Day = tomorrow.Day,
            Month = tomorrow.Month,
            Year = tomorrow.Year
        };

        // Act
        var result = _sut.Validate(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        });
    }
}
