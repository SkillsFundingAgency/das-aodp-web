using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Views.Shared;

[ExcludeFromCodeCoverage]
public record BackLinkModel
{
    public string? Area { get; set; } 

    public string? Controller { get; set; } 

    public string? Action { get; set; }

    public string? Text { get; set; } = "Back";
}