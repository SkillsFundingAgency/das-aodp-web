using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

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
        public async Task UploadQualificationCandidates_Post_WhenFileNullAndSessionHasCandidates_RedirectsToFundingStreamSelection()
        {
            var session = CreateEmptySession();
            var rollover = new Rollover
            {
                RolloverCandidates = new List<QualificationCandidate>
                {
                    new QualificationCandidate()
                }
            };
            session.SetString("RolloverSession", JsonConvert.SerializeObject(rollover));

            var controller = CreateController(session);

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = null
            };

            var result = await controller.UploadQualificationCandidates(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("FundingStreamInclusionExclusion", redirect.ActionName);
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

            var model = new RolloverUploadQualificationCandidatesViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csv = new CsvFileReaderResult<QualificationCandidate>();
            csv.Errors.Add("Bad CSV");

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    model.File,
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
                    model.File,
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
                    model.File,
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
                    model.File,
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
            var saved = JsonConvert.DeserializeObject<Rollover>(json);

            Assert.NotNull(saved);
            Assert.NotEmpty(saved.RolloverCandidates);
        }
    }
}
