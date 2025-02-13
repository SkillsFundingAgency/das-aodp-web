using SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers;

namespace SFA.DAS.AODP.Authentication.Tests.Api.Helpers
{
    public class JsonWebAlgorithmTests
    {
        [Theory]
        [InlineData("HMACSHA1", "HS1")]
        [InlineData("HMACSHA256", "HS256")]
        [InlineData("HMACSHA384", "HS384")]
        [InlineData("HMACSHA512", "HS512")]
        public void Then_Algorithm_Is_Correctly_Mapped(string algorithm, string expected)
        {
            // Act
            var result = JsonWebAlgorithm.GetAlgorithm(algorithm);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}