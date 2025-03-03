using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Users;

public class GetApplicationsForReviewQuery : IRequest<BaseMediatrResponse<GetApplicationsForReviewQueryResponse>>
{
    public string? ApplicationSearch { get; set; }
    public string? AwardingOrganisationSearch { get; set; }
    public List<ApplicationStatus>? ApplicationStatuses { get; set; } = new();
    public string ReviewUser { get; set; }


    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public bool ApplicationsWithNewMessages { get; set; }
}
