using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SFA.DAS.AODP.Web.TagHelpers;
using System.Text.Encodings.Web;

namespace SFA.DAS.AODP.Web.UnitTests.TagHelpers;

public class FormGroupTagHelperUnitTests : TagHelpersUnitTestBase
{
    private class TestModel
    {
        public string TestProperty { get; set; } = null!;
    }

    [Fact]
    public void FormGroupTagHelper_EnsureGeneratesDivWithGdsStyling_NoFormError()
    {
        // Arrange
        var model = new TestModel()
        {
            TestProperty = "some value"
        };

        SetModelMetadata<TestModel>();

        var tagHelper = new FormGroupTagHelper()
        {
            AspFor = new ModelExpression(
                nameof(model.TestProperty),
                new ModelExplorer(MockModelMetaDataProvider.Object,
               MockModelMetadata.Object, model)),
            ViewContext = CreateDefaultViewContext()
        };

        var tagHelperContext = GenerateTagHelperContext(FormGroupTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(FormGroupTagHelper.TagName, new TagHelperAttributeList());

        // Act
        tagHelper.Process(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<div class=\"govuk-form-group\"></div>", writer.ToString());
    }

    [Fact]
    public void FormGroupTagHelper_EnsureGeneratesDivWithGdsStyling_WithFormErrors()
    {
        // Arrange
        var model = new TestModel()
        {
            TestProperty = "some value"
        };

        SetModelMetadata<TestModel>();
        var viewContext = CreateDefaultViewContext();
        viewContext.ModelState.AddModelError(nameof(model.TestProperty), "error");

        var tagHelper = new FormGroupTagHelper
        {
            AspFor = new ModelExpression(
                nameof(model.TestProperty),
                new ModelExplorer(MockModelMetaDataProvider.Object,
               MockModelMetadata.Object, model)),
            ViewContext = viewContext
        };

        var tagHelperContext = GenerateTagHelperContext(FormGroupTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(FormGroupTagHelper.TagName, new TagHelperAttributeList());

        // Act
        tagHelper.Process(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<div class=\"govuk-form-group govuk-form-group--error\"></div>", writer.ToString());
    }
}