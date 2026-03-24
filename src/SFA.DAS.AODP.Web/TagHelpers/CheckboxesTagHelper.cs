using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.AODP.Web.TagHelpers;

[HtmlTargetElement(TagName, Attributes = AspForAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
public class CheckboxesTagHelper : TagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;
    private const string AspForAttributeName = "asp-for";

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="htmlGenerator">Provides access to the builtin HTML generator that can generate common tag helpers.</param>
    public CheckboxesTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator;
    }

    /// <summary>
    /// Defines the name of the tag that is to be used within the markup to use this tag helper.
    /// </summary>
    public const string TagName = "checkboxes";

    /// <summary>
    /// Context for the view execution.
    /// </summary>
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    /// <summary>
    /// The model expression that describes the model property that the tag helper is associated with.
    /// </summary>
    [HtmlAttributeName(AspForAttributeName)]
    public required ModelExpression For { get; set; }

    /// <summary>
    /// Collection of checkbox items to be created.
    /// </summary>
    public List<CheckboxItem> Items { get; set; } = new();

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.SuppressOutput();

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "govuk-checkboxes");
        output.Attributes.SetAttribute("data-module", "govuk-checkboxes");

        var contentBuilder = new HtmlContentBuilder();

        if (Items.Count > 0)
        {
            var index = 0;
            foreach (var checkboxItem in Items)
            {
                var divItemContainer = new TagBuilder("div")
                {
                    Attributes = { { "class", "govuk-checkboxes__item" } }
                };

                var id = index == 0 ? For.Name : $"{For.Name}-{index}";

                var checkbox = _htmlGenerator.GenerateCheckBox(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    checkboxItem.IsChecked,
                    new
                    {
                        @class = "govuk-checkboxes__input",
                        @value = checkboxItem.Value,
                        id
                    });

                var label = _htmlGenerator.GenerateLabel(
                    ViewContext,
                    For.ModelExplorer,
                    id,
                    checkboxItem.LabelText,
                    new
                    {
                        @class = "govuk-label govuk-checkboxes__label"
                    });

                divItemContainer.InnerHtml.AppendHtml(checkbox);
                divItemContainer.InnerHtml.AppendHtml(label);
                contentBuilder.AppendHtml(divItemContainer);

                index++;
            }
        }

        output.Content.AppendHtml(contentBuilder);
    }
}