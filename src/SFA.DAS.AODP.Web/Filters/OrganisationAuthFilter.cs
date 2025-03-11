using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Filters
{
    public class ValidateOrganisationAttribute : TypeFilterAttribute
    {
        public ValidateOrganisationAttribute() : base(typeof(OrganisationAuthFilter))
        {
        }
    }


    public class OrganisationAuthFilter : Attribute, IAuthorizationFilter
    {

        private readonly IUserHelperService _userHelperService;

        public OrganisationAuthFilter(IUserHelperService userHelperService)
        {
            _userHelperService = userHelperService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var orgId = _userHelperService.GetUserOrganisationId();

                if (Guid.TryParse(context.RouteData.Values["organisationId"]?.ToString(), out var routeOrgId))
                {
                    if (routeOrgId.ToString() != orgId)
                        throw new Exception("Organisation id in route is not valid for user");
                }            
            }
            catch
            {
                context.Result = new BadRequestResult();
            }

        }
    }

}