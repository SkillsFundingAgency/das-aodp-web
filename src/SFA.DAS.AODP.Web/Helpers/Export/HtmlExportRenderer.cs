using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SFA.DAS.AODP.Web.Helpers.Export;
public interface IHtmlExportRenderer
{
    Task<string> RenderAsync(string viewName, object model);
}

public class HtmlExportRenderer : IHtmlExportRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HtmlExportRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RenderAsync(string viewName, object model)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException(
                "No active HttpContext available.");
        }

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData(),
            httpContext.GetEndpoint()?.Metadata.GetMetadata<ActionDescriptor>()
                ?? new ActionDescriptor());

        using var sw = new StringWriter();

        var viewResult = _viewEngine.FindView(actionContext, viewName, false);

        if (!viewResult.Success)
            throw new InvalidOperationException(
                $"View '{viewName}' not found. " +
                $"Searched locations:{Environment.NewLine}" +
                string.Join(Environment.NewLine, viewResult.SearchedLocations));

        var viewDictionary = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            tempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);

        return sw.ToString();
    }

}
