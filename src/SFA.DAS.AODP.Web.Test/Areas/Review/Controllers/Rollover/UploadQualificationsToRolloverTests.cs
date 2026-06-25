using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

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
            Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenCsvInvalid_ReturnsCorrectViewWithErrors()
        {
            var controller = CreateController(CreateEmptySession());

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
                    model.File,
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationFails_ReturnsValidationErrorsView()
        {
            var controller = CreateController(CreateEmptySession());

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
                    model.File,
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
            Assert.Equal("RolloverValidationErrors", view.ViewName);

            var returnedModel = Assert.IsType<RolloverUploadQualificationsViewModel>(view.Model);
            Assert.NotNull(returnedModel.ValidationSummary);
            Assert.Equal(1, returnedModel.ValidationSummary.FailedCandidateCount);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationSucceeds_RedirectsToSummary()
        {
            var controller = CreateController(CreateEmptySession());

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
                    model.File,
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

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>(),
                ReturnViewName = "UploadQualificationsToRollover"
            };

            CsvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Func<IReadOnlyDictionary<string, string>, FundingExtensionCandidate>>()))
                .ThrowsAsync(new Exception("le exception"));

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("UploadQualificationsToRollover", view.ViewName);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey(""));
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
