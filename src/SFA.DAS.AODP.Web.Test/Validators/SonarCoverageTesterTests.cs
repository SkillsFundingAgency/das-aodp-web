using SFA.DAS.AODP.Web.Models;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class SonarCoverageTesterTests
{
    [Theory]
    [InlineData(-1, "Apple", "Negative - Found Apple")]  // Tests Negative branch
    [InlineData(5, "Banana", "Low Range - Found Banana")] // Tests Low Range branch
    [InlineData(50, "Cherry", "Medium Range - Found Cherry")] // Tests Medium Range
    [InlineData(150, "Apple", "High Range - Found Apple")] // Tests High Range
    [InlineData(5, "Other", "Unknown Category")] // Tests 'default' switch case
    public void EvaluateValue_ShouldCoverAllLogicalBranches(int input, string category, string expected)
    {
        // Arrange
        var tester = new SonarCoverageTester();

        // Act
        var result = tester.EvaluateValue(input, category);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EvaluateValue_ShouldHandleCaseInsensitiveCategories()
    {
        // Arrange
        var tester = new SonarCoverageTester();

        // Act & Assert
        // This specifically hits the "test" case in the switch statement
        Assert.Equal("LOW RANGE - FOUND APPLE", tester.EvaluateValue(5, "test"));

        // This specifically hits the "demo" case in the switch statement
        Assert.Equal("low range", tester.EvaluateValue(5, "demo"));
    }
}