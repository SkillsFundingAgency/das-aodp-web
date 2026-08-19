using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Helpers.Export;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ApplicationsReviewControllerDeleteTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ApplicationsReviewController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly ApplicationsReviewController _controller;

    public ApplicationsReviewControllerDeleteTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<ApplicationsReviewController>>>();
        _userHelperMock = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _fileServiceMock = _fixture.Freeze<Mock<IFileService>>();

        var aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        _controller = new ApplicationsReviewController(
            _loggerMock.Object,
            _mediatorMock.Object,
            _userHelperMock.Object,
            _fileServiceMock.Object,
            aodpOptions,
            Mock.Of<IApplicationExportService>());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        _userHelperMock
            .Setup(x => x.GetUserType())
            .Returns(UserType.Qfau);

        _fileServiceMock
            .Setup(x => x.ListBlobs(It.IsAny<string>()))
            .Returns(new List<UploadedBlob>());
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewWithApplicationDetails()
    {
        var applicationReviewId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();

        var review = new GetApplicationForReviewByIdQueryResponse
        {
            Id = applicationId,
            Name = "Test qualification",
            Reference = 123456
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationForReviewByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse> { Success = true, Value = review });

        var result = await _controller.Delete(applicationReviewId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DeleteApplicationReviewViewModel>(viewResult.Model);

        Assert.Equal(applicationReviewId, model.ApplicationReviewId);
        Assert.Equal(applicationId, model.ApplicationId);
        Assert.Equal(review.Name, model.ApplicationName);
        Assert.Equal(review.Reference, model.ApplicationReference);
    }

    [Fact]
    public async Task Delete_Post_DeletesApplicationAsQfauUser_AndRedirectsToIndex_WithSuccessFlag()
    {
        var model = new DeleteApplicationReviewViewModel
        {
            ApplicationReviewId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            ApplicationName = "Test qualification",
            ApplicationReference = 123456
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true, Value = new EmptyResponse() });

        var result = await _controller.Delete(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationsReviewController.Index), redirect.ActionName);
        Assert.True((bool)_controller.TempData["ApplicationDeleted"]!);

        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteApplicationCommand>(c =>
                c.ApplicationId == model.ApplicationId &&
                c.UserType == UserType.Qfau.ToString()),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_Post_DeletesFilesUnderBothApplicationAndMessagesPrefixes()
    {
        var model = new DeleteApplicationReviewViewModel
        {
            ApplicationReviewId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            ApplicationName = "Test qualification",
            ApplicationReference = 123456
        };

        var applicationFile = new UploadedBlob { FullPath = $"{model.ApplicationId}/abc123", FileName = "answer.pdf" };
        var messageFile = new UploadedBlob { FullPath = $"messages/{model.ApplicationId}/msg1/attachment.pdf", FileName = "attachment.pdf" };

        _fileServiceMock
            .Setup(x => x.ListBlobs(model.ApplicationId.ToString()))
            .Returns(new List<UploadedBlob> { applicationFile });

        _fileServiceMock
            .Setup(x => x.ListBlobs($"messages/{model.ApplicationId}"))
            .Returns(new List<UploadedBlob> { messageFile });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true, Value = new EmptyResponse() });

        await _controller.Delete(model);

        _fileServiceMock.Verify(x => x.DeleteFileAsync(applicationFile.FullPath), Times.Once);
        _fileServiceMock.Verify(x => x.DeleteFileAsync(messageFile.FullPath), Times.Once);
    }

    [Fact]
    public async Task Delete_Post_WhenSendFails_ReturnsViewWithModel()
    {
        var model = new DeleteApplicationReviewViewModel
        {
            ApplicationReviewId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            ApplicationName = "Test qualification",
            ApplicationReference = 123456
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteApplicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = false, ErrorMessage = "The application has been submitted" });

        var result = await _controller.Delete(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
    }
}
