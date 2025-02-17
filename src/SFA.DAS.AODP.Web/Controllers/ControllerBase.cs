﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application;

namespace SFA.DAS.AODP.Web.Controllers;

public class ControllerBase(IMediator mediator, ILogger logger) : Controller
{
    protected readonly IMediator _mediator = mediator;
    protected readonly ILogger _logger = logger;

    public async Task<TResponse> Send<TResponse>(IRequest<BaseMediatrResponse<TResponse>> request) where TResponse : class, new()
    {
        var response = await _mediator.Send(request);
        if (!response.Success)
        {
            ViewBag.InternalServerError = true;
            _logger.LogError(response.ErrorMessage);

            throw new Exception(response.ErrorMessage);
        }
        return response.Value;
    }
}
