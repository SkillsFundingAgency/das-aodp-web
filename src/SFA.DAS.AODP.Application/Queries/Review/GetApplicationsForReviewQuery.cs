﻿using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationsForReviewQuery : IRequest<BaseMediatrResponse<GetApplicationsForReviewQueryResponse>>
{
    public string? ApplicationSearch { get; set; }
    public string? AwardingOrganisationSearch { get; set; }
    public List<string>? ApplicationStatuses { get; set; } = new();
    public string ReviewUser { get; set; }


    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public bool ApplicationsWithNewMessages { get; set; }
}
