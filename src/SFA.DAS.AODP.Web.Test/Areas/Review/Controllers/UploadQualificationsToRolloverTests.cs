using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class UploadQualificationsToRolloverTests
    {
        private readonly Mock<ICsvFileReader> _csvFileReaderMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<RolloverController>> _loggerMock;
        private readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> _eligibilityDatesValidatorMock;
        private readonly Mock<IValidator<RolloverFundingApprovalEndDateViewModel>> _approvalEndDateValidatorMock;
        private readonly Mock<IUserHelperService> _userHelperServiceMock;

        public UploadQualificationsToRolloverTests()
        {
            _csvFileReaderMock = new Mock<ICsvFileReader>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<RolloverController>>();
            _eligibilityDatesValidatorMock = new Mock<IValidator<RolloverEligibilityDatesViewModel>>();
            _approvalEndDateValidatorMock = new Mock<IValidator<RolloverFundingApprovalEndDateViewModel>>();
            _userHelperServiceMock = new Mock<IUserHelperService>();
        }

        private RolloverController CreateController(ISession session)
        {
            var controller = new RolloverController(
                _loggerMock.Object,
                _mediatorMock.Object,
                _eligibilityDatesValidatorMock.Object,
                _approvalEndDateValidatorMock.Object,
                _csvFileReaderMock.Object,
                _userHelperServiceMock.Object);

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

        private static ISession EmptySession() => new TestSession();

        [Fact]
        public async Task UploadQualificationsToRollover_WhenFileNullAndSessionHasCandidates_RedirectsToSummary()
        {
            var session = EmptySession();
            var rollover = new Rollover
            {
                RolloverFundingExtensionCandidates = new List<FundingExtensionCandidate>()
            };
            session.SetString("RolloverSession", JsonConvert.SerializeObject(rollover));

            var controller = CreateController(session);

            var model = new RolloverUploadQualificationsViewModel
            {
                File = null
            };

            var result = await controller.UploadQualificationsToRollover(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RolloverSummary", redirect.ActionName);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_InvalidModelState_ReturnsViewWithModel()
        {
            var controller = CreateController(EmptySession());
            controller.ModelState.AddModelError("File", "required");

            var model = new RolloverUploadQualificationsViewModel();

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenCsvInvalid_ReturnsViewWithErrors()
        {
            var controller = CreateController(EmptySession());

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            var csvResult = new CsvFileReaderResult<FundingExtensionCandidate>
            {
                Errors = { "Bad row", "Missing column" }
            };

            _csvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    model.File,
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey("File"));
        }


        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationFails_RedirectsToValidationErrors()
        {
            var controller = CreateController(EmptySession());

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>()
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

            _csvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    model.File,
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ValidateFundingExtensionCandidatesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>
                {
                    Success = true,
                    Value = new ValidateFundingExtensionCandidatesCommandResponse
                    {
                        IsValid = false,
                        Candidates = new List<CandidateValidationResult>()
                    }
                });

            var result = await controller.UploadQualificationsToRollover(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RolloverValidationErrors", redirect.ActionName);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenValidationSucceeds_RedirectsToSummary()
        {
            var controller = CreateController(EmptySession());

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>()
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

            _csvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    model.File,
                    FundingExtensionCandidateColumns.Required,
                    FundingExtensionCandidateMapper.Map))
                .ReturnsAsync(csvResult);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ValidateFundingExtensionCandidatesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>
                {
                    Success = true,
                    Value = new ValidateFundingExtensionCandidatesCommandResponse
                    {
                        IsValid = true,
                        Candidates = new List<CandidateValidationResult>()
                    }
                });

            var result = await controller.UploadQualificationsToRollover(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RolloverSummary", redirect.ActionName);
        }

        [Fact]
        public async Task UploadQualificationsToRollover_WhenExceptionThrown_ReturnsViewWithError()
        {
            var controller = CreateController(EmptySession());

            var model = new RolloverUploadQualificationsViewModel
            {
                File = Mock.Of<IFormFile>()
            };

            _csvFileReaderMock
                .Setup(x => x.FileReadAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string[]>(),
                    It.IsAny<Func<IReadOnlyDictionary<string, string>, FundingExtensionCandidate>>()))
                .ThrowsAsync(new Exception("le exception"));

            var result = await controller.UploadQualificationsToRollover(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(model, view.Model);
            Assert.True(controller.ModelState.ContainsKey(""));
        }

        [Fact]
        public void RolloverSummary_ReturnsView()
        {
            var controller = CreateController(EmptySession());

            var result = controller.RolloverSummary();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void RolloverValidationErrors_ReturnsView()
        {
            var controller = CreateController(EmptySession());

            var result = controller.RolloverValidationErrors();

            Assert.IsType<ViewResult>(result);
        }
    }
}
