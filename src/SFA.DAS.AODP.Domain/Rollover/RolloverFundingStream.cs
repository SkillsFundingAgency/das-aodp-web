namespace SFA.DAS.AODP.Domain.Rollover;

public class RolloverFundingStream
{
    public List<FundingStreamDto> FundingStreams { get; set; } = new();
    public List<Guid> SelectedIds { get; set; } = new();
}

public class FundingStreamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}