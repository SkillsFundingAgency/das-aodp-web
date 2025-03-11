using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Helpers.User
{
    public interface IUserHelperService
    {
        string GetUserDisplayName();
        string GetUserEmail();
        string GetUserOrganisationId();
        string GetUserOrganisationName();
        string GetUserOrganisationUkPrn();
        UserType GetUserType();
    }
}