using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Views.Shared;

[ExcludeFromCodeCoverage]
public record BackLinkModel
{
    private string? _controller;
    public string? Area { get; set; }

    public string? Controller
    {
        get => _controller;
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.EndsWith("Controller"))
                {
                    _controller = value.Replace("Controller", "");
                    return;
                }
            }

            _controller = value;
        }
    }

    public string? Action { get; set; }

    public string? Text { get; set; } = "Back";
}