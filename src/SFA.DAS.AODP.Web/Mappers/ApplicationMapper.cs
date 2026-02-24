using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Mappers;

public static class ApplicationMapper
{
    public static List<ApplicationModel> Map(GetApplicationsByQanQueryResponse response)
    {
        List<ApplicationModel> applications = new();
        foreach (var application in response.Applications.OrderByDescending(x => x.SubmittedDate).ThenByDescending(x => x.ReferenceId))
        {
            applications.Add(new()
            {
                Id = application.Id,
                Name = application.ReferenceId.ToString().PadLeft(6, '0'),
                CreatedDate = application.CreatedDate,
                SubmittedDate = application.SubmittedDate,
                Status = ApplicationStatusDisplay.Dictionary[application.Status],
                ReferenceId = application.ReferenceId,
                ApplicationReviewId = application.ApplicationReviewId
            });
        }
        return applications;
    }
}
