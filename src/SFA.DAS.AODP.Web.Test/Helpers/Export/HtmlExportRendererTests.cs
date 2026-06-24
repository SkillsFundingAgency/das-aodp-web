using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SFA.DAS.AODP.Web.Helpers.Export;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.Export
{
    public class HtmlExportRendererTests
    {
        private readonly Mock<IRazorViewEngine> _viewEngineMock;
        private readonly Mock<ITempDataProvider> _tempDataProviderMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        private readonly HtmlExportRenderer _renderer;

        public HtmlExportRendererTests()
        {
            _viewEngineMock = new Mock<IRazorViewEngine>();
            _tempDataProviderMock = new Mock<ITempDataProvider>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _renderer = new HtmlExportRenderer(
                _viewEngineMock.Object,
                _tempDataProviderMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task RenderAsync_ReturnsRenderedHtml()
        {
            var httpContext = new DefaultHttpContext();

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(httpContext);

            var viewMock = new Mock<IView>();

            viewMock.Setup(x => x.RenderAsync(It.IsAny<ViewContext>()))
                .Returns<ViewContext>(vc =>
                {
                    vc.Writer.Write("rendered-html");
                    return Task.CompletedTask;
                });

            _viewEngineMock.Setup(x =>
                x.FindView(It.IsAny<ActionContext>(), "test-view", false))
                .Returns(ViewEngineResult.Found("test-view", viewMock.Object));

            var result = await _renderer.RenderAsync("test-view", new { });

            Assert.Equal("rendered-html", result);
        }

        [Fact]
        public async Task RenderAsync_WhenHttpContextIsNull_ThrowsException()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            try
            {
                await _renderer.RenderAsync("test-view", new { });
            }
            catch (Exception ex)
            {
                Assert.Equal("No active HttpContext available.", ex.Message);
            }
        }

        [Fact]
        public async Task RenderAsync_WhenViewNotFound_ThrowsException()
        {
            var httpContext = new DefaultHttpContext();

            _httpContextAccessorMock.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _viewEngineMock.Setup(x =>
                x.FindView(It.IsAny<ActionContext>(), "missing-view", false))
                .Returns(ViewEngineResult.NotFound("missing-view", new List<string>()));

            try
            {
                await _renderer.RenderAsync("missing-view", new { });
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message.Contains("View 'missing-view' not found."));
            }
        }
    }
}