using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Interfaces;

namespace SFA.DAS.AODP.Web.Authentication;
public class CustomServiceRole : ICustomServiceRole
{
    public string RoleClaimType => "http://schemas.portal.com/service";
    public CustomServiceRoleValueType RoleValueType => CustomServiceRoleValueType.Code;
}
