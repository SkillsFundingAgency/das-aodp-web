using SFA.DAS.AODP.Models.Users;

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
            return UserType.Qfau;
        }

        public string GetUserOrganisationId()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUserOrganisationName()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUserOrganisationUkPrn()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUserDisplayName()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetUserEmail()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
