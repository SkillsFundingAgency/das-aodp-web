using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Filters
{
    public class ValidateApplicationAttribute : TypeFilterAttribute
    {
        public ValidateApplicationAttribute() : base(typeof(ApplicationAuthFilter))
        {
        }
    }


    public class ApplicationAuthFilter : Attribute, IAsyncAuthorizationFilter
    {
        private readonly IMediator _mediator;
        private readonly IUserHelperService _userHelperService;

        public ApplicationAuthFilter(IMediator mediator, IUserHelperService userHelperService)
        {
            _mediator = mediator;
            _userHelperService = userHelperService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var routeApplicationId = Guid.Parse(context.RouteData.Values["applicationId"].ToString());
                var orgId = _userHelperService.GetUserOrganisationId();

                var response = await _mediator.Send(new GetApplicationMetadataByIdQuery(routeApplicationId));
                if (!response.Success) throw new Exception(response.ErrorMessage);

                if (Guid.TryParse(context.RouteData.Values["formVersionId"]?.ToString(), out var routeFormId))
                {
                    if (routeFormId != response.Value.FormVersionId)
                        throw new Exception("Form id for application does not match route application id");
                }


                if (orgId != response.Value.OrganisationId.ToString())
                    throw new Exception("Organisation id for application does not match user's organisation id");

            }
            catch
            {
                context.Result = new BadRequestResult();
            }

        }
    }

}