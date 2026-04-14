using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Web.TagHelpers;

/// <summary>
/// Defines a TagHelper that renders an anchor tag with GDS styling, linking to the Ofqual Register for a specified qualification reference.
/// The link can be configured to open in a named tab or a new tab based on the 'opens-in-named-tab' attribute.
/// </summary>
[HtmlTargetElement(TagName, Attributes = "qualification-reference")]
public class OfqualRegisterExternalLinkTagHelper(IOptions<AodpConfiguration> aodpConfiguration) : TagHelper
{
    private readonly AodpConfiguration _aodpConfiguration = aodpConfiguration.Value;
    public const string TagName = "ofqual-register";

    /// <summary>
    /// The qualification reference to link to on the Ofqual Register. This attribute is required and must not be empty.
    /// </summary>
    [HtmlAttributeName("qualification-reference")]
    public string QualificationReference { get; set; } = null!;

    /// <summary>
    /// Determines whether the link should open in a named tab (using the qualification reference as the target) or in a new tab (using "_blank" as the target).
    /// </summary>
    [HtmlAttributeName("opens-in-named-tab")]
    public bool OpensInNamedTab { get; set; } = true;

    /// <inheritdoc/>
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        base.ProcessAsync(context, output);

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("class", "govuk-link");

        if (string.IsNullOrEmpty(QualificationReference))
        {
            throw new TagHelperRenderingException(nameof(OfqualRegisterExternalLinkTagHelper), "The 'qualification-reference' attribute must be provided and cannot be empty.");
        }

        if (OpensInNamedTab)
        {
            output.Attributes.SetAttribute("target", $"{QualificationReference}");
        }
        else
        {
            output.Attributes.SetAttribute("target", "_blank");
        }

        if (string.IsNullOrEmpty(_aodpConfiguration.FindRegulatedQualificationUrl))
        {
            throw new TagHelperRenderingException(nameof(OfqualRegisterExternalLinkTagHelper), "The 'FindRegulatedQualificationUrl' configuration value must be provided and cannot be empty.");
        }

        output.Attributes.SetAttribute("href", $"{_aodpConfiguration.FindRegulatedQualificationUrl}{QualificationReference}");
        output.Content.SetContent(QualificationReference);

        return Task.CompletedTask;
    }
}