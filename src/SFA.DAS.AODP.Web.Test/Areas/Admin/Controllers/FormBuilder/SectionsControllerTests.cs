using AutoFixture;
using AutoFixture.AutoMoq;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using SFA.DAS.AODP.Web.Models.FormBuilder.Section;
using System.Reflection.Metadata;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class SectionsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<SectionsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SectionsController _controller;

    public SectionsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<SectionsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new SectionsController(_mediatorMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task Delete_Get_ReturnsViewModel()
    {
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var queryResponse = new BaseMediatrResponse<GetSectionByIdQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetSectionByIdQueryResponse>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSectionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);
        // Act
        var result = await _controller.Delete(sectionId, formVersionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DeleteSectionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionId, model.SectionId);
        Assert.Equal(queryResponse.Value.HasAssociatedRoutes, model.HasAssociatedRoutes);
        Assert.Equal(queryResponse.Value.Title, model.Title);
    }

    [Fact]
    public async Task Delete_Post_ReturnsRedirectToListPage()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var model = new DeleteSectionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteSectionCommandResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSectionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.TempData = new Mock<ITempDataDictionary>().Object;

        // Act
        var result = await _controller.DeleteConfirmed(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);
        Assert.Equal("Forms", redirect.ControllerName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var model = new DeleteSectionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteSectionCommandResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSectionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.DeleteConfirmed(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<DeleteSectionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, routeModel.FormVersionId);
        Assert.Equal(sectionId, routeModel.SectionId);
    }
}
