using Moq;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Requests;

public class GetQualificationsApiRequestTests
{
    [Fact]
    public void GetUrl_IncludesSearchTermSkipAndTake_WhenAllValuesProvided()
    {
        // Arrange
        var request = new GetQualificationsApiRequest("term", 0, 25);

        // Act
        var url = request.GetUrl;

        // Assert - insertion order in the NameValueCollection is preserved by AttachParameters
        Assert.Equal("api/qualifications/GetMatchingQualifications?SearchTerm=term&Skip=0&Take=25", url);
    }

    [Fact]
    public void GetUrl_OnlyIncludesSearchTerm_WhenSkipAndTakeAreNull()
    {
        // Arrange
        var request = new GetQualificationsApiRequest("only", null, null);

        // Act
        var url = request.GetUrl;

        // Assert
        Assert.Equal("api/qualifications/GetMatchingQualifications?SearchTerm=only", url);
    }

    [Theory]
    [InlineData(null, "api/qualifications/GetMatchingQualifications?SearchTerm=")]
    [InlineData("", "api/qualifications/GetMatchingQualifications?SearchTerm=")]
    [InlineData("   ", "api/qualifications/GetMatchingQualifications?SearchTerm=   ")]
    public void GetUrl_PreservesSearchTermNullEmptyOrWhitespace(string searchTerm, string expected)
    {
        // Arrange
        var request = new GetQualificationsApiRequest(searchTerm, 0, 25);

        // Act
        var url = request.GetUrl;

        // Assert
        Assert.Equal(expected + "&Skip=0&Take=25".Replace("&Skip=0&Take=25", "&Skip=0&Take=25"), url);
        // above Replace keeps expected concatenation explicit and clear in the assertion
    }

    [Fact]
    public void GetUrl_DoesNotUrlEncodeValues_AttachesRawSearchTerm()
    {
        // Arrange - search term contains characters that would normally be encoded
        var special = "a b&c=1";
        var request = new GetQualificationsApiRequest(special, 1, 2);

        // Act
        var url = request.GetUrl;

        // Assert - AttachParameters appends values raw (no encoding)
        Assert.Equal($"api/qualifications/GetMatchingQualifications?SearchTerm={special}&Skip=1&Take=2", url);
    }

    [Fact]
    public void GetUrl_UsesModifiedBaseUrl_WhenBaseUrlChanged()
    {
        // Arrange
        var request = new GetQualificationsApiRequest("x", 5, 10)
        {
            BaseUrl = "https://example.com/custom/endpoint"
        };

        // Act
        var url = request.GetUrl;

        // Assert
        Assert.Equal("https://example.com/custom/endpoint?SearchTerm=x&Skip=5&Take=10", url);
    }

    [Fact]
    public void Moq_IsAvailable_Demonstration()
    {
        // Demonstrates Moq is referenced and usable in the test project.
        var mock = new Mock<object>();
        Assert.NotNull(mock.Object);
    }
}
