using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse
{
    public IEnumerable<AwardingOrganisation> AwardingOrganisations { get; set; } = [];
}
