using Newtonsoft.Json;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Authentication;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.Helpers.User
{
    public class UserHelperService : IUserHelperService
    {
        private readonly IHttpContextAccessor _http;

        public UserHelperService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public UserType GetUserType()
        {
            var roles = GetUserClaims("rolecode");

            foreach (var role in roles)
            {
                if(RoleConstants.AoRoles.Contains(role.Value)) return UserType.AwardingOrganisation;
                if(RoleConstants.IfateRoles.Contains(role.Value)) return UserType.SkillsEngland;
                if(RoleConstants.OfqualRoles.Contains(role.Value)) return UserType.Ofqual;
                if(RoleConstants.QfauRoles.Contains(role.Value)) return UserType.Qfau;
            }

            throw new Exception("No user type could be identified");
        }

        public string GetUserOrganisationId()
        {
            Claim orgClaim = GetUserClaim("organisation");
            var claimOrgId = JsonConvert.DeserializeObject<UserOrganisation>(orgClaim.Value).Id;
            return claimOrgId.ToString();
        }

        public string GetUserOrganisationName()
        {
            return GetUserClaim("organisationName").Value;
        }

        public string GetUserOrganisationUkPrn()
        {
            return GetUserClaim("ukPrn").Value;
        }

        public string GetUserDisplayName()
        {
            return GetUserClaim(ClaimTypes.Name).Value;
        }

        public string GetUserEmail()
        {
            return GetUserClaim("email").Value;
        }

        private Claim GetUserClaim(string claimType)
        {
            return _http.HttpContext.User.Claims.Where(c => c.Type == claimType).First();
        }

        private List<Claim> GetUserClaims(string claimType)
        {
            return _http.HttpContext.User.Claims.Where(c => c.Type == claimType).ToList();
        }
    }
}
