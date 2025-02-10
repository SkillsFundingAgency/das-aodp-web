using Moq;
using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Interfaces;

namespace SFA.DAS.AODP.Authentication.Tests.MiddlewareConfig
{
    public class WhenAddingCustomServiceRoleTest
    {
        [Theory]
        [InlineData(CustomServiceRoleValueType.Code, null)]
        [InlineData(CustomServiceRoleValueType.Name, "http://schemas.portal.com/displayname")]
        public void Then_The_Properties_Are_Correctly_Returned(CustomServiceRoleValueType roleValue, string roleClaim)
        {
            // arrange
            var customServiceRole = new Mock<ICustomServiceRole>();
            customServiceRole.Setup(c => c.RoleValueType).Returns(roleValue);
            customServiceRole.Setup(c => c.RoleClaimType).Returns(roleClaim);

            // assert
            Assert.Equal(roleClaim, customServiceRole.Object.RoleClaimType);
        }
    }
}
