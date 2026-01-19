using Moq;
using SFA.DAS.AODP.Web.Models;

namespace SFA.DAS.AODP.Web.Test.Validators;

public class SonarCoverageTesterTests
{
    [Theory]
    [InlineData(-1, "Apple", "Negative - Found Apple (Standard)")] // Negative + Match + ToUpper
    [InlineData(5, "Banana", "Low Range - Found Banana (Standard)")] // Low + Match + ToLower
    [InlineData(50, "Cherry", "Medium Range - Found Cherry (Standard)")] // Med + Match + Default
    [InlineData(150, "Unknown", "High Range (Standard)")] // High + No Match + Default
    public void EvaluateValue_ShouldReturnExpectedResults(int input, string category, string expected)
    {
        // Arrange
        var tester = new SonarCoverageTester();

        // Act
        var result = tester.EvaluateValue(input, category);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Moq_ProofOfIntegration()
    {
        // Simple Moq usage to ensure your Moq reference is working in the PR
        var mock = new Mock<IDisposable>();
        mock.Setup(m => m.Dispose()).Verifiable();

        mock.Object.Dispose();

        mock.Verify(m => m.Dispose(), Times.Once);
    }
}