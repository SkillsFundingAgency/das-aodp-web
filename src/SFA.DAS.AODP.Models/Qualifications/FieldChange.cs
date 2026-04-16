using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Models.Qualifications
{
    [ExcludeFromCodeCoverage]
    public record FieldChange(
        string Name,
        string? Was,
        string? Now
    );
}
