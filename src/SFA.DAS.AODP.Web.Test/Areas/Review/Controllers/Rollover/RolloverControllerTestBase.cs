using System.Text;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Infrastructure.Cache;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers.Rollover;

public abstract class RolloverControllerTestBase
{
    protected readonly Mock<ICsvFileReader> CsvFileReaderMock = new();
    protected readonly Mock<IMediator> MediatorMock = new();
    protected readonly Mock<ILogger<RolloverController>> LoggerMock = new();
    protected readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> EligibilityDatesValidatorMock = new();
    protected readonly Mock<IValidator<RolloverFundingApprovalEndDateViewModel>> ApprovalEndDateValidatorMock = new();
    protected readonly Mock<IUserHelperService> UserHelperServiceMock = new();
    protected readonly Mock<ICacheService> CacheServiceMock = new();
    protected readonly Mock<IFileService> FileServiceMock = new();
    protected readonly Mock<IDelayService> DelayServiceMock = new();

    protected RolloverControllerTestBase()
    {
        DelayServiceMock
            .Setup(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected RolloverController CreateController(ISession session)
    {
        var controller = new RolloverController(
            LoggerMock.Object,
            MediatorMock.Object,
            EligibilityDatesValidatorMock.Object,
            ApprovalEndDateValidatorMock.Object,
            CsvFileReaderMock.Object,
            UserHelperServiceMock.Object,
            CacheServiceMock.Object,
            FileServiceMock.Object,
            DelayServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    protected static ISession CreateEmptySession()
    {
        var session = new TestSession();
        session.Set("RolloverSession",
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new AODP.Domain.Rollover.Rollover())));

        return session;
    }

    protected static ISession CreateThrowingSessionOnGet() => new ThrowingSession(throwOnGet: true, throwOnSet: false);
    protected static ISession CreateThrowingSessionOnSet() => new ThrowingSession(throwOnGet: false, throwOnSet: true);

    // Sets up the upload -> create record -> scan-check chain so it succeeds on the very first
    // check, for tests concerned with what happens after a file is confirmed scanned rather
    // than with the wait itself.
    protected void SetupSuccessfulScan(
        SFA.DAS.Aodp.Domain.Files.FileCategory category = SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateImport,
        string blobPath = "Rollover/test.csv")
    {
        FileServiceMock
            .Setup(f => f.UploadAsync(
                category,
                null,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>()))
            .ReturnsAsync(new FileStorageLocation("importfilescontainer", blobPath));

        FileServiceMock
            .Setup(f => f.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Stream.Null);

        MediatorMock
            .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true });

        MediatorMock
            .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetFileMetadataQuery query, CancellationToken _) => new BaseMediatrResponse<GetFileMetadataQueryResponse>
            {
                Success = true,
                Value = new GetFileMetadataQueryResponse
                {
                    Files = new List<Application.Queries.Files.FileMetadataDto>
                    {
                        new() { FileId = query.FileId!.Value, IsDownloadable = true }
                    }
                }
            });
    }
}