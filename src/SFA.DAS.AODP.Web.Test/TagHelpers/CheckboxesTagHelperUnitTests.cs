using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using System.Text.Encodings.Web;
using SFA.DAS.AODP.Web.TagHelpers;

namespace SFA.DAS.AODP.Web.UnitTests.TagHelpers;

public class CheckboxesTagHelperUnitTests : TagHelpersUnitTestBase
{
    private class TestModel
    {
        public string TestProperty { get; set; } = null!;
    }

    [Fact]
    public void CheckboxesTagHelper_EnsureGeneratesDivWithGdsStyling()
    {
        // Arrange
        var viewContext = CreateDefaultViewContext();
        SetModelMetadata<TestModel>();
        var model = new TestModel();
        var modelExplorer = new ModelExplorer(MockModelMetaDataProvider.Object, MockModelMetadata.Object, model);
        var mockHtmlGenerator = new Mock<IHtmlGenerator>();
        var modelExpression = new ModelExpression(nameof(model.TestProperty), modelExplorer);
        var tagHelper = new CheckboxesTagHelper(mockHtmlGenerator.Object)
        {
            For = modelExpression,
            ViewContext = viewContext,
            Items =
            [
                new()
                {
                    Value = "value",
                    LabelText = "text"
                },
                new()
                {
                    Value = "value 2",
                    LabelText = "label 2"
                }
            ]
        };

        var tagHelperContext = GenerateTagHelperContext(CheckboxesTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(CheckboxesTagHelper.TagName, new TagHelperAttributeList());

        // Expectations
        mockHtmlGenerator
            .Setup(o => o.GenerateCheckBox(It.IsAny<ViewContext>(), It.IsAny<ModelExplorer>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<object>())).Returns(new TagBuilder("checkbox"));

        mockHtmlGenerator
            .Setup(o => o.GenerateLabel(It.IsAny<ViewContext>(), It.IsAny<ModelExplorer>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<object>())).Returns(new TagBuilder("label"));

        // Act
        tagHelper.Process(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal("<div class=\"govuk-checkboxes\" data-module=\"govuk-checkboxes\"><div class=\"govuk-checkboxes__item\"><checkbox></checkbox><label></label></div><div class=\"govuk-checkboxes__item\"><checkbox></checkbox><label></label></div></div>", writer.ToString());
    }
}