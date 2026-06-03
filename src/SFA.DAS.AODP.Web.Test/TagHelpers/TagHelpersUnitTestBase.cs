using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace SFA.DAS.AODP.Web.UnitTests.TagHelpers;

public class TagHelpersUnitTestBase
{
    protected Mock<IModelMetadataProvider> MockModelMetaDataProvider = new();

    protected Mock<ModelMetadata> MockModelMetadata { get; set; } = null!;

    protected void SetModelMetadata<T>() => MockModelMetadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(T)));

    protected static TagHelperContext GenerateTagHelperContext(string tagName, string uniqueId = "test")
        => new(
            tagName: tagName,
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");

    protected static TagHelperOutput GenerateTagHelperOutput(string tagName, TagHelperAttributeList attributes)
        => new(
            tagName,
            attributes: attributes,
            getChildContentAsync: (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

    protected static ViewContext CreateDefaultViewContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ViewContext(
            actionContext,
            Mock.Of<IView>(),
            new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary()),
            Mock.Of<ITempDataDictionary>(),
            TextWriter.Null,
            new HtmlHelperOptions());
    }
}