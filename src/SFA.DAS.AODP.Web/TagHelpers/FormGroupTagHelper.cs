using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.AODP.Web.TagHelpers;

/// <summary>
/// Defines a tag helper that renders a <div></div> html tag that generates a form group that acts as a wrapper for a form control.
/// </summary>
[HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing, Attributes = AspForAttributeName)]
public class FormGroupTagHelper : TagHelper
{
    private const string AspForAttributeName = "asp-for";

    /// <summary>
    /// The model expression that is bound to this tag helper.
    /// </summary>
    [HtmlAttributeName(AspForAttributeName)]
    public ModelExpression AspFor { get; set; } = null!;

    /// <summary>
    /// The tag name used in the markup to create this element.
    /// </summary>
    public const string TagName = "form-group";

    /// <summary>
    /// Context around the view that this tag helper is being used in.
    /// </summary>
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc/>>.
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);

        output.TagName = "div";
        var cssClass = "govuk-form-group";

        if (ViewContext.ModelState[AspFor.Name]?.Errors.Count > 0)
        {
            cssClass += " govuk-form-group--error";
        }

        output.Attributes.SetAttribute("class", cssClass);
    }
}