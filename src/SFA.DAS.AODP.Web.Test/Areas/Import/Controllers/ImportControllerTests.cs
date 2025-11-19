using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Web.Areas.Import.Controllers;
using SFA.DAS.AODP.Web.Areas.Import.Models;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Import.Controllers;

public class ImportControllerTests
{
    private const string IndexViewPath = "~/Areas/Import/Views/DefundingList/Index.cshtml";
    private const string ImportedViewPath = "~/Areas/Import/Views/DefundingList/Imported.cshtml";

    private readonly Fixture _fixture = new();
    private readonly Mock<IMediator> mediator = new();
    private readonly Mock<ILogger<ImportController>> logger = new();

    [Fact]
    public void When_GetIndexView_ShouldReturnIndexView()
    {
        // Arrange
        var controller = new ImportController(mediator.Object, logger.Object);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(IndexViewPath, viewResult.ViewName);
    }

    [Fact]
    public async Task When_Post_Index_ModelStateInvalid_Should_Return_IndexViewWithModel()
    {
        // Arrange
        var controller = new ImportController(mediator.Object, logger.Object);

        var model = new UploadDefundingListViewModel();
        controller.ModelState.AddModelError("some", "some error");

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(IndexViewPath, viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task When_Post_Index_FileNull_Should_Return_IndexViewWithModelError()
    {
        // Arrange
        var controller = new ImportController(mediator.Object, logger.Object);

        var model = new UploadDefundingListViewModel
        {
            File = null
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(IndexViewPath, viewResult.ViewName);
        Assert.True(controller.ModelState.ContainsKey(nameof(model.File)));
        var entry = controller.ModelState[nameof(model.File)];
        Assert.Contains(entry.Errors, e => e.ErrorMessage.Contains(".xlsx", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task When_Post_Index_FileLengthZero_Should_Return_IndexViewWithModelError()
    {
        // Arrange
        var controller = new ImportController(mediator.Object, logger.Object);

        var emptyFile = CreateFormFile("test.xlsx", Array.Empty<byte>());

        var model = new UploadDefundingListViewModel
        {
            File = emptyFile
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(IndexViewPath, viewResult.ViewName);
        Assert.True(controller.ModelState.ContainsKey(nameof(model.File)));
        var entry = controller.ModelState[nameof(model.File)];
        Assert.Contains(entry.Errors, e => e.ErrorMessage.Contains(".xlsx", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task When_Post_Index_WrongExtension_Should_Returns_IndexViewWithModelError()
    {
        // Arrange
        var controller = new ImportController(mediator.Object, logger.Object);

        var txtFile = CreateFormFile("test.txt", new byte[] { 1, 2, 3 });

        var model = new UploadDefundingListViewModel
        {
            File = txtFile
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(IndexViewPath, viewResult.ViewName);
        Assert.True(controller.ModelState.ContainsKey(nameof(model.File)));
        var entry = controller.ModelState[nameof(model.File)];
        Assert.Contains(entry.Errors, e => e.ErrorMessage.Contains(".xlsx", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task When_Post_Index_ValidFile_Should_Returns_ImportedView()
    {
        // Arrange

        var msgResponse = new BaseMediatrResponse<ImportDefundingListResponse>()
        {
            Success = true,
            Value = _fixture.Create<ImportDefundingListResponse>()
        };
        mediator
            .Setup(m => m.Send(It.IsAny<ImportDefundingListCommand>(), default))
            .ReturnsAsync(msgResponse);

        var controller = new ImportController(mediator.Object, logger.Object);

        var xlsxFile = CreateFormFile("list.xlsx", new byte[] { 1, 2, 3 });

        var model = new UploadDefundingListViewModel
        {
            File = xlsxFile
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(ImportedViewPath, viewResult.ViewName);
    }

    [Fact]
    public async Task When_Post_Index_MediatorThrows_Should_Returns_ViewWithModelError()
    {
        // Arrange
        mediator
            .Setup(m => m.Send(It.IsAny<IRequest<object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error"));

        mediator
            .Setup(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error"));

        var controller = new ImportController(mediator.Object, logger.Object);

        var xlsxFile = CreateFormFile("list.xlsx", new byte[] { 1, 2, 3 });

        var model = new UploadDefundingListViewModel
        {
            File = xlsxFile
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
        Assert.True(controller.ModelState.ContainsKey(string.Empty));
        var entry = controller.ModelState[string.Empty];
        Assert.Contains(entry.Errors, e => e.ErrorMessage.Contains("unexpected", StringComparison.OrdinalIgnoreCase));
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "File", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }
}
