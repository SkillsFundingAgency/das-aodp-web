using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class FormsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<FormsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly FormsController _controller;

    public FormsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<FormsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new FormsController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewModel()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var queryResponse = new BaseMediatrResponse<GetFormVersionByIdQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetFormVersionByIdQueryResponse>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetFormVersionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);
        // Act
        var result = await _controller.Delete(formVersionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DeleteFormViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(queryResponse.Value.FormId, model.FormId);
        Assert.Equal(queryResponse.Value.Title, model.Title);

    }

    [Fact]
    public async Task Delete_Post_ReturnsRedirectToIndexPage()
    {
        // Arrange
        var model = _fixture.Create<DeleteFormViewModel>();

        var queryResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteFormCommand>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.TempData = new Mock<ITempDataDictionary>().Object;

        // Act
        var result = await _controller.Delete(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var model = _fixture.Create<DeleteFormViewModel>();

        var queryResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteFormCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Delete(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var responseModel = Assert.IsAssignableFrom<DeleteFormViewModel>(viewResult.ViewData.Model);
        Assert.Equal(model.FormId, responseModel.FormId);
        Assert.Equal(model.FormVersionId, responseModel.FormVersionId);
        Assert.Equal(model.Title, responseModel.Title);

    }

}
