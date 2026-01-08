namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQueryResponse
{
    public int TotalRecords { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public List<QualificationSearchResult> Data { get; set; } = new();
    //public Job Job { get; set; } = new();
}

public class QualificationSearchResult
{
    public string? Reference { get; set; }
    public string? Title { get; set; }
    public string? AwardingOrganisation { get; set; }
    public string? Status { get; set; }
    public string? AgeGroup { get; set; }
}
