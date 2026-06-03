using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

[ExcludeFromCodeCoverage]
public record KeyFieldChanges(string? Name, string? Was, string? Now);