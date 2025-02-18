using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

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

        public ApplicationAuthFilter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var routeApplicationId = Guid.Parse(context.RouteData.Values["applicationId"].ToString());
                var routeFormId = Guid.Parse(context.RouteData.Values["formVersionId"].ToString());
                var routeOrgId = Guid.Parse(context.RouteData.Values["organisationId"].ToString());

                var response = await _mediator.Send(new GetApplicationMetadataByIdQuery(routeApplicationId));
                if (!response.Success) throw new Exception(response.ErrorMessage);

                if (routeFormId != response.Value.FormVersionId)
                    throw new Exception("Form id for application does not match route application id");


                if (routeOrgId != response.Value.OrganisationId)
                    throw new Exception("Organisation id for application does not match route organisation id");

            }
            catch
            {
                //context.Result = new BadRequestResult();

                //TODO remove this try catch once internal testing completed
            }

        }
    }

}