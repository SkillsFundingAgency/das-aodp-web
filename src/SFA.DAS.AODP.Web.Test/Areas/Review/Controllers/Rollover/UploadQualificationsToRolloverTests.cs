using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class UploadQualificationsToRolloverTests : RolloverControllerTestBase
    {
        [Fact]
        public async Task UploadQualificationsToRollover_InvalidModelState_ReturnsCorrectView()
        {
            var controller = CreateController(CreateEmptySession());
            controller.ModelState.AddModelError("File", "required");

            var model = new RolloverUploadQualificationsViewModel
            {
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            //Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenCsvInvalid_ReturnsCorrectViewWithErrors()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan(SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted, "RolloverSubmitted/test.csv");

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var csvResult = new CsvFileReaderResult<FundingExtensionCandidate>
            {
                Errors = { "Bad row", "Missing column" }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            //Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationFails_ReturnsValidationErrorsView()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan(SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted, "RolloverSubmitted/test.csv");

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var csvResult = new CsvFileReaderResult<FundingExtensionCandidate>
            {
                Items =
                {
                    new FundingExtensionCandidate
                    {
                        Qan = "123",
                        FundingStreamName = "FS",
                        ProposedFundingApprovalEndDate = DateTime.UtcNow,
                        RollOverStatus = "Extend",
                    }
                }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            MediatorMock
                .Setup(m => m.Send(It.IsAny<ValidateRolloverExtensionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>
                {
                    Success = true,
                    Value = new ValidateRolloverExtensionCommandResponse
                    {
                        IsValid = false,
                        ValidationFailureSummary = new ValidationFailureSummary
                        {
                            FailedCandidateCount = 1,
                            ValidatedCandidateFile = new byte[] { 0x01, 0x02, 0x03 },
                        }
                    }
                });

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            //Assert.Equal("RolloverValidationErrors", view.ViewName);

            var returnedModel = Assert.IsType<RolloverUploadQualificationsViewModel>(view.Model);
            Assert.NotNull(returnedModel.ValidationSummary);
            Assert.Equal(1, returnedModel.ValidationSummary.FailedCandidateCount);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationSucceeds_RedirectsToSummary()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan(SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted, "RolloverSubmitted/test.csv");

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var csvResult = new CsvFileReaderResult<FundingExtensionCandidate>
            {
                Items =
                {
                    new FundingExtensionCandidate
                    {
                        Qan = "123",
                        FundingStreamName = "FS",
                        ProposedFundingApprovalEndDate = DateTime.UtcNow,
                        RollOverStatus = "Extend"
                    }
                }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            MediatorMock
                .Setup(m => m.Send(It.IsAny<ValidateRolloverExtensionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>
                {
                    Success = true,
                    Value = new ValidateRolloverExtensionCommandResponse
                    {
                        IsValid = true,
                        ValidationSuccessSummary = new FundingExtensionSummary()
                    }
                });

            var result = await controller.UploadQualificationsToRollover(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RolloverSummary", redirect.ActionName);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenExceptionThrown_ReturnsCorrectViewWithError()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan(SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted, "RolloverSubmitted/test.csv");

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Func<IReadOnlyDictionary<string, string>, FundingExtensionCandidate>>()))
                .ThrowsAsync(new Exception("le exception"));

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            //Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenScanNeverCompletes_ReturnsViewWithError()
        {
            var controller = CreateController(CreateEmptySession());

            FileServiceMock
                .Setup(f => f.UploadAsync(
                    SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted,
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>()))
                .ReturnsAsync(new FileStorageLocation("importfilescontainer", "RolloverSubmitted/test.csv"));

            MediatorMock
                .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true });

            // Never becomes downloadable, however many times it's checked.
            MediatorMock
                .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetFileMetadataQueryResponse>
                {
                    Success = true,
                    Value = new GetFileMetadataQueryResponse { Files = new List<FileMetadataDto>() }
                });

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));

            // Never reads the file content if it was never confirmed safe.
            CsvFileReaderMock.Verify(
                x => x.FileReadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(),
                    It.IsAny<IEnumerable<string>>(), It.IsAny<Func<IReadOnlyDictionary<string, string>, FundingExtensionCandidate>>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_UploadsBeforeReading_AndReadsFromTheUploadedLocation()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan(SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted, "RolloverSubmitted/test.csv");

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            var csvResult = new CsvFileReaderResult<FundingExtensionCandidate>();
            csvResult.Errors.Add("doesn't matter for this test");

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            await controller.UploadQualificationsToRollover(model);

            FileServiceMock.Verify(f => f.UploadAsync(
                SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted,
                null,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>()),
                Times.Once);

            MediatorMock.Verify(m => m.Send(
                It.Is<CreateFileMetadataCommand>(c => c.FileCategory == SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateSubmitted
                    && c.BlobContainer == "importfilescontainer"
                    && c.BlobPath == "RolloverSubmitted/test.csv"),
                It.IsAny<CancellationToken>()),
                Times.Once);

            FileServiceMock.Verify(f => f.OpenReadStreamAsync("importfilescontainer", "RolloverSubmitted/test.csv"), Times.Once);
        }

        [Fact]
        public void RolloverSummary_ReturnsView()
        {
            var controller = CreateController(CreateEmptySession());

            var result = controller.RolloverSummary(new RolloverSummaryViewModel { });

            Assert.IsType<ViewResult>(result);
        }
    }
}
