
namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsQueryResponse
    {
            public List<ChangedQualification> ChangedQualifications { get; set; } = new();
        public class ChangedQualification
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Reference { get; set; }
            public string? AwardingOrganisation { get; set; }
            public string? Status { get; set; }
        }
    }
}
