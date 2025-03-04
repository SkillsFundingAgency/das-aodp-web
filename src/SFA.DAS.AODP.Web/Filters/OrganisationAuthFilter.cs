using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.AODP.Models.Users;

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
       

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var orgClaim = context.HttpContext.User.Claims.Where(c => c.Type == "organisation").First();
                var claimOrgId = JsonConvert.DeserializeObject<UserOrganisation>(orgClaim.Value).Id;

                var routeOrgId = Guid.Parse(context.RouteData.Values["organisationId"].ToString());

                if (claimOrgId == default) throw new Exception("Invalid organisation id in user claims");
                if (routeOrgId == default) throw new Exception("Invalid organisation id in route");

                if (claimOrgId != routeOrgId) throw new Exception("Organisation id in user claims does not match route organisation id");
            }
            catch
            {
                //context.Result = new BadRequestResult();
                //TODO remove this try catch once internal testing completed
            }

        }
    }

}