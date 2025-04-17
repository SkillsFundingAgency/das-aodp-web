using SFA.DAS.AODP.Authentication.Enums;

namespace SFA.DAS.DfESignIn.Auth.UnitTests.Enums
{
    public class CustomServiceRoleValueTypeTest
    {
        [Fact]
        public void Then_The_Properties_Are_Correctly_Returned()
        {
            var properties = CustomServiceRoleEnumValues().ToList();
            Assert.True(properties.Count > 0);
        }

        private static IEnumerable<object[]> CustomServiceRoleEnumValues()
        {
            return from object? number in Enum.GetValues(typeof(CustomServiceRoleValueType)) select new object[] { number };
        }
    }
}
