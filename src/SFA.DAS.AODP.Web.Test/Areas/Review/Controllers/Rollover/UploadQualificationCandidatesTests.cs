using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class UploadQualificationCandidatesTests : RolloverControllerTestBase
    {
        [Fact]
        public async Task UploadQualificationCandidates_Get_ReturnsViewWithEmptyModel()
        {
            var controller = CreateController(CreateEmptySession());

            var result = await controller.UploadQualificationCandidates();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<RolloverUploadQualificationCandidatesViewModel>(view.Model);
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_InvalidModelState_ReturnsView()
        {
            var controller = CreateController(CreateEmptySession());
            controller.ModelState.AddModelError("File", "required");

            var model = new RolloverUploadQualificationCandidatesViewModel();

            var result = await controller.UploadQualificationCandidates(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_WhenCsvInvalid_ReturnsViewWithErrors()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan();

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>();
            csv.Errors.Add("Bad CSV");

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    QualificationImportColumns.Required,
                    QualificationCandidateMapper.Map))
                .ReturnsAsync(csv);

            var result = await controller.UploadQualificationCandidates(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_WhenMediatorThrows_StillContinues()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan();

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>
            {
                Items = { new QualificationCandidate { QualificationNumber = "123" } }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    QualificationImportColumns.Required,
                    QualificationCandidateMapper.Map))
                .ReturnsAsync(csv);

            MediatorMock
                .Setup(m => m.Send(It.IsAny<GetRolloverCandidatesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("boom"));

            var result = await controller.UploadQualificationCandidates(model);

            // Should still return a view or redirect depending on match logic
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_WhenNoMatchesFound_ReturnsViewWithError()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan();

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>
            {
                Items = { new QualificationCandidate { QualificationNumber = "999" } }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    QualificationImportColumns.Required,
                    QualificationCandidateMapper.Map))
                .ReturnsAsync(csv);

            MediatorMock
                .Setup(m => m.Send(It.IsAny<GetRolloverCandidatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>
                {
                    Success = true,
                    Value = new GetRolloverCandidatesQueryResponse
                    {
                        RolloverCandidates = new List<RolloverCandidate>() // no matches
                    }
                });

            var result = await controller.UploadQualificationCandidates(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("File"));
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_WhenMatchesFound_SavesSessionAndRedirects()
        {
            var session = CreateEmptySession();
            var controller = CreateController(session);
            SetupSuccessfulScan();

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>
            {
                Items = { new QualificationCandidate { QualificationNumber = "123" } }
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    QualificationImportColumns.Required,
                    QualificationCandidateMapper.Map))
                .ReturnsAsync(csv);

            MediatorMock
                .Setup(m => m.Send(It.IsAny<GetRolloverCandidatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>
                {
                    Success = true,
                    Value = new GetRolloverCandidatesQueryResponse
                    {
                        RolloverCandidates = new List<RolloverCandidate>
                        {
                            new RolloverCandidate { QualificationNumber = "123" }
                        }
                    }
                });

            var result = await controller.UploadQualificationCandidates(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FundingStreamInclusionExclusion", redirect.ActionName);

            Assert.True(session.TryGetValue("RolloverSession", out var bytes));
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var saved = JsonConvert.DeserializeObject<AODP.Domain.Rollover.Rollover>(json);

            Assert.NotNull(saved);
            Assert.NotEmpty(saved.RolloverCandidates);
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_WhenScanNeverCompletes_ReturnsViewWithError()
        {
            var controller = CreateController(CreateEmptySession());

            FileServiceMock
                .Setup(f => f.UploadAsync(
                    SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateImport,
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>()))
                .ReturnsAsync(new FileStorageLocation("importfilescontainer", "Rollover/test.csv"));

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

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var result = await controller.UploadQualificationCandidates(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));

            // Never reads the file content if it was never confirmed safe.
            CsvFileReaderMock.Verify(
                x => x.FileReadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(),
                    It.IsAny<IEnumerable<string>>(), It.IsAny<Func<IReadOnlyDictionary<string, string>, QualificationCandidate>>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadQualificationCandidates_Post_UploadsBeforeReading_AndReadsFromTheUploadedLocation()
        {
            var controller = CreateController(CreateEmptySession());
            SetupSuccessfulScan();

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>();
            csv.Errors.Add("doesn't matter for this test");

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    QualificationImportColumns.Required,
                    QualificationCandidateMapper.Map))
                .ReturnsAsync(csv);

            await controller.UploadQualificationCandidates(model);

            FileServiceMock.Verify(f => f.UploadAsync(
                SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateImport,
                null,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Stream>()),
                Times.Once);

            MediatorMock.Verify(m => m.Send(
                It.Is<CreateFileMetadataCommand>(c => c.FileCategory == SFA.DAS.Aodp.Domain.Files.FileCategory.RolloverCandidateImport
                    && c.BlobContainer == "importfilescontainer"
                    && c.BlobPath == "Rollover/test.csv"),
                It.IsAny<CancellationToken>()),
                Times.Once);

            FileServiceMock.Verify(f => f.OpenReadStreamAsync("importfilescontainer", "Rollover/test.csv"), Times.Once);
        }
    }
}
