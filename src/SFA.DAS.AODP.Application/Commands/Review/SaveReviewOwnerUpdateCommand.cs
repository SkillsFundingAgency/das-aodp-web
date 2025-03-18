﻿using MediatR;
using SFA.DAS.AODP.Application;

public class SaveReviewOwnerUpdateCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationReviewId { get; set; }
    public string? Owner { get; set; }
    public string UserType { get; set; }
    public string SentByName { get; set; }
    public string SentByEmail { get; set; }
}
