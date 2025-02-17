using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application;

namespace SFA.DAS.AODP.Web.Controllers;

public class ControllerBase(IMediator mediator) : Controller
{
    protected readonly IMediator _mediator = mediator;

    public async Task<TResponse> Send<TResponse>(IRequest<BaseMediatrResponse<TResponse>> request) where TResponse : class, new()
    {
        var response = await _mediator.Send(request);
        if (!response.Success)
        {
            ViewBag.InternalServerError = true;
            throw new Exception();
        }
        return response.Value;
    }
}
