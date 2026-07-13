using System.Collections.Specialized;
using SFA.DAS.AODP.Common.Extensions;

namespace SFA.DAS.AODP.Common.Tests.Extensions
{
    public class UriExtensionTests
    {
        [Fact]
        public void AttachParameters_Uri_Appends_Query_Params()
        {
            // Arrange
            var uri = new Uri("https://test.com/api");

            var parameters = new NameValueCollection
            {
                { "Skip", "10" },
                { "Take", "20" }
            };

            // Act
            var result = uri.AttachParameters(parameters);

            // Assert
            Assert.Equal(
                "https://test.com/api?Skip=10&Take=20",
                result.ToString());
        }

        [Fact]
        public void AttachParameters_String_Appends_Query_Params()
        {
            // Arrange
            var uri = "api/qualifications";

            var parameters = new NameValueCollection
            {
                { "Status", "New" },
                { "Skip", "5" }
            };

            // Act
            var result = uri.AttachParameters(parameters);

            // Assert
            Assert.Equal(
                "api/qualifications?Status=New&Skip=5",
                result);
        }

        [Fact]
        public void AttachMultiValueParameters_Appends_Multiple_Values_For_Same_Key()
        {
            // Arrange
            var uri = "api/qualifications";

            var parameters = new NameValueCollection
            {
                { "AgeGroups", "0" },
                { "AgeGroups", "2" }
            };

            // Act
            var result = uri.AttachMultiValueParameters(parameters);

            // Assert
            Assert.Equal(
                "api/qualifications?AgeGroups=0&AgeGroups=2",
                result);
        }

        [Fact]
        public void AttachMultiValueParameters_Encodes_Keys_And_Values()
        {
            // Arrange
            var uri = "api/test";

            var parameters = new NameValueCollection
            {
                { "Full Name", "City & Guilds" }
            };

            // Act
            var result = uri.AttachMultiValueParameters(parameters);

            // Assert
            Assert.Equal(
                "api/test?Full%20Name=City%20%26%20Guilds",
                result);
        }

        [Fact]
        public void AttachMultiValueParameters_Ignores_Null_Values()
        {
            // Arrange
            var uri = "api/test";

            var parameters = new NameValueCollection();

            // Act
            var result = uri.AttachMultiValueParameters(parameters);

            // Assert
            Assert.Equal("api/test", result);
        }
    }
}