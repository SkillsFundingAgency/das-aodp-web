namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQueryResponse
{
    public int TotalRecords { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public List<GetMatchingQualificationsQueryItem> Qualifications { get; set; } = new();
}

public class GetMatchingQualificationsQueryItem
{
    public Guid Id { get; set; }
    public string Qan { get; set; } = null!;
    public string? QualificationName { get; set; }

    public Guid? Status { get; set; }
}
