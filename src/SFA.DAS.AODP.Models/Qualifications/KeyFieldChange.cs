using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Models.Qualifications;

/// <summary>
/// Represents a change to a field, optionally associated with a key field definition.
/// Can be used for general field changes or specifically for key field changes with priority.
/// </summary>
[ExcludeFromCodeCoverage]
public record KeyFieldChange(KeyField KeyField, string? Was, string? Now);
