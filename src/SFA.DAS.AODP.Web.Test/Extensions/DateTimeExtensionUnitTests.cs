using SFA.DAS.AODP.Web.Extensions;

namespace SFA.DAS.AODP.Web.UnitTests.Extensions;

public class DateTimeExtensionUnitTests : UnitTest
{
    [Fact]
    public void ToDateTimeDisplayFormat_MorningTime_ReturnsCorrectFormatWithAm()
    {
        // Arrange - 22 May 2026 at 08:15:30 AM
        var testDate = new DateTime(2026, 5, 22, 8, 15, 30);
        var expected = "22 May 2026 at 08:15am";

        // Act
        var result = testDate.ToDateTimeDisplayFormat();

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ToDateTimeDisplayFormat_EveningTime_ReturnsCorrectFormatWithPm()
    {
        // Arrange - 22 May 2026 at 08:15:30 PM (20:15)
        var testDate = new DateTime(2026, 5, 22, 20, 15, 30);
        var expected = "22 May 2026 at 08:15pm";

        // Act
        var result = testDate.ToDateTimeDisplayFormat();

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ToDateDisplayFormat_ValidDate_ReturnsLongDateString()
    {
        // Arrange
        var testDate = new DateTime(2026, 5, 22);
        var expected = "22 May 2026";

        // Act
        var result = testDate.ToDateDisplayFormat();

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(13, 5, "22/05/2026 01:05:00 pm")] // Afternoon conversion
    [InlineData(0, 0, "22/05/2026 12:00:00 am")]  // Midnight edge case
    [InlineData(12, 0, "22/05/2026 12:00:00 pm")] // Noon edge case
    public void ToStandard12HourDateTimeFormat_VariousTimes_ReturnsCorrectStrings(int hour, int minute, string expected)
    {
        // Arrange
        var testDate = new DateTime(2026, 5, 22, hour, minute, 0);

        // Act
        var result = testDate.ToStandard12HourDateTimeFormat();

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }
}