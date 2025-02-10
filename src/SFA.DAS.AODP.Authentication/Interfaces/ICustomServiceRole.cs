using SFA.DAS.AODP.Authentication.Enums;

namespace SFA.DAS.AODP.Authentication.Interfaces
{
    public interface ICustomServiceRole
    {
        string RoleClaimType { get; }

        /// <summary>
        /// Property defines the custom service role value type(Name/Code) when mapping the claims to the identity.
        /// </summary>
        CustomServiceRoleValueType RoleValueType { get; }
    }
}
