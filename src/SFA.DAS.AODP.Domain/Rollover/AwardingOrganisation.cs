using SFA.DAS.AODP.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record AwardingOrganisation
{
    public Guid Id { get; set; }
    public int? Ukprn { get; set; }
    public string? RecognitionNumber { get; set; }
    public string? NameLegal { get; set; }
    public string? NameOfqual { get; set; }
    public string? NameGovUk { get; set; }
    public string? Name_Dsi { get; set; }
    public string? Acronym { get; set; }
}