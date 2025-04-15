using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;

namespace SFA.DAS.AODP.Web.Test.Areas.Admin.Controllers.FormBuilder;

public class FormControllerTests
{
    private readonly Fixture _fixture = new();
    private readonly FormsController _sut;
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<FormsController>> _logger = new();

    public FormControllerTests()
    {
        _sut = new(_mediator.Object, _logger.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewModel()
    {
        // Arrange
        var response = _fixture.Create<BaseMediatrResponse<GetAllFormVersionsQueryResponse>>();
        response.Success = true;

        _mediator.Setup(m => m.Send(It.IsAny<GetAllFormVersionsQuery>(), default))
            .ReturnsAsync(response);

        // Act
        IActionResult actual = await _sut.Index();

        // Assert
        ViewResult? viewResult = actual as ViewResult;
        Assert.NotNull(viewResult);

        FormVersionListViewModel? result = viewResult.ViewData.Model as FormVersionListViewModel;
        Assert.NotNull(result);
    }
}