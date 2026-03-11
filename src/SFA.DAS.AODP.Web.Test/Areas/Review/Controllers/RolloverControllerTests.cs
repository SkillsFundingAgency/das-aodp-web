using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class RolloverControllerTests
{
    private readonly Mock<ILogger<RolloverController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> _validatorMock;
    private readonly Mock<ICsvFileReader> _csvFileReaderMock;

    public RolloverControllerTests()
    {
        _loggerMock = new Mock<ILogger<RolloverController>>();
        _mediatorMock = new Mock<IMediator>();
        _validatorMock = new Mock<IValidator<RolloverEligibilityDatesViewModel>>();
        _csvFileReaderMock = new Mock<ICsvFileReader>();
    }

    private static ISession CreateEmptySession() => new TestSession();
    private static ISession CreateThrowingSessionOnGet() => new ThrowingSession(throwOnGet: true, throwOnSet: false);
    private static ISession CreateThrowingSessionOnSet() => new ThrowingSession(throwOnGet: false, throwOnSet: true);

    private RolloverController CreateControllerWithSession(ISession session)
    {
        var controller = new RolloverController(_loggerMock.Object, _mediatorMock.Object, _validatorMock.Object, _csvFileReaderMock.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Session = session;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    [Fact]
    public void Index_Get_ReturnsRolloverStartView_WithEmptyModel_WhenNoSession()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        var model = Assert.IsType<RolloverStartViewModel>(viewResult.Model);
        Assert.Null(model.SelectedProcess);
    }

    [Fact]
    public void Index_Get_HandlesSessionGetException_ReturnsView()
    {
        var controller = CreateControllerWithSession(CreateThrowingSessionOnGet());

        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
    }

    [Fact]
    public void Index_Get_WhenSessionHasStart_PopulatesModel()
    {
        var session = CreateEmptySession();
        var sessionModel = new Rollover { Start = new RolloverStart { SelectedProcess = RolloverProcess.FinalUpload } };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<RolloverStartViewModel>(viewResult.Model);
        Assert.Equal(RolloverProcess.FinalUpload, vm.SelectedProcess);
    }

    [Fact]
    public void Index_Post_InvalidModelState_ReturnsStartView_WithSameModel()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());
        controller.ModelState.AddModelError("SelectedProcess", "required");

        var vm = new RolloverStartViewModel();

        var result = controller.Index(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        Assert.Same(vm, viewResult.Model);
    }

    [Fact]
    public void Index_Post_SelectedProcessInitialSelection_SavesSessionAndRedirects()
    {
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverStartViewModel { SelectedProcess = RolloverProcess.InitialSelection };

        var result = controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.CheckData), redirect.ActionName);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var sessionModel = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(sessionModel?.Start);
        Assert.Equal(RolloverProcess.InitialSelection, sessionModel!.Start!.SelectedProcess);
    }

    [Fact]
    public void Index_Post_SelectedProcessFinalUpload_SavesSessionAndRedirects()
    {
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverStartViewModel { SelectedProcess = RolloverProcess.FinalUpload };

        var result = controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.UploadQualifications), redirect.ActionName);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var sessionModel = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(sessionModel?.Start);
        Assert.Equal(RolloverProcess.FinalUpload, sessionModel!.Start!.SelectedProcess);
    }

    [Fact]
    public void Index_Post_SaveSessionThrows_DoesNotBubbleException_Redirects()
    {
        var session = CreateThrowingSessionOnSet();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverStartViewModel { SelectedProcess = RolloverProcess.InitialSelection };

        var result = controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.CheckData), redirect.ActionName);
    }

    [Fact]
    public void UploadQualifications_Get_SetsTitle()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());

        var result = controller.UploadQualifications();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Upload qualifications to RollOver", viewResult.ViewData["Title"]);
    }

    [Fact]
    public async Task CheckData_Get_WhenSessionHasImportStatus_MapsFromSession_AndReturnsView()
    {
        var session = CreateEmptySession();
        var sessionModel = new Rollover
        {
            ImportStatus = new RolloverImportStatus
            {
                RegulatedQualificationsLastImported = new DateTime(2025, 2, 2),
                FundedQualificationsLastImported = new DateTime(2025, 2, 3),
                DefundingListLastImported = new DateTime(2025, 2, 4),
                PldnsListLastImported = new DateTime(2025, 2, 5),
            }
        };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));
        var controller = CreateControllerWithSession(session);

        var result = await controller.CheckData();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
        var vm = Assert.IsType<RolloverImportStatusViewModel>(viewResult.Model);
        Assert.Equal(sessionModel.ImportStatus.RegulatedQualificationsLastImported, vm.RegulatedQualificationsLastImported);
        Assert.Equal(sessionModel.ImportStatus.FundedQualificationsLastImported, vm.FundedQualificationsLastImported);
        Assert.Equal(sessionModel.ImportStatus.DefundingListLastImported, vm.DefundingListLastImported);
        Assert.Equal(sessionModel.ImportStatus.PldnsListLastImported, vm.PldnsListLastImported);
    }

    [Fact]
    public async Task CheckData_Get_WhenNoSession_CallsMediatorAndSavesSession()
    {
        // arrange
        var regulatedDate = new DateTime(2025, 11, 25, 10, 0, 0);
        var fundedDate = new DateTime(2025, 11, 26, 11, 0, 0);
        var defundingDate = new DateTime(2025, 11, 27, 12, 0, 0);
        var pldnsDate = new DateTime(2025, 11, 27, 13, 0, 0);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetJobRunsQuery q, CancellationToken ct) =>
            {
                var resp = new BaseMediatrResponse<GetJobRunsQueryResponse>
                {
                    Success = true,
                    Value = new GetJobRunsQueryResponse
                    {
                        JobRuns = new List<JobRun>()
                    }
                };

                var run = new JobRun
                {
                    Id = Guid.NewGuid(),
                    Status = "Completed",
                    StartTime = DateTime.MinValue,
                    EndTime = q.JobName switch
                    {
                        var s when s == JobNames.RegulatedQualifications.ToString() => regulatedDate,
                        var s when s == JobNames.FundedQualifications.ToString() => fundedDate,
                        var s when s == JobNames.DefundingList.ToString() => defundingDate,
                        var s when s == JobNames.Pldns.ToString() => pldnsDate,
                        _ => null
                    },
                    User = "tester",
                    JobId = Guid.NewGuid()
                };

                if (run.EndTime != null)
                {
                    resp.Value.JobRuns.Add(run);
                }

                return resp;
            });

        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var result = await controller.CheckData();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
        var model = Assert.IsType<RolloverImportStatusViewModel>(viewResult.Model);
        Assert.Equal(regulatedDate, model.RegulatedQualificationsLastImported);
        Assert.Equal(fundedDate, model.FundedQualificationsLastImported);
        Assert.Equal(defundingDate, model.DefundingListLastImported);
        Assert.Equal(pldnsDate, model.PldnsListLastImported);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(saved?.ImportStatus);
        Assert.Equal(regulatedDate, saved!.ImportStatus!.RegulatedQualificationsLastImported);
        Assert.Equal(fundedDate, saved.ImportStatus.FundedQualificationsLastImported);
        Assert.Equal(defundingDate, saved.ImportStatus.DefundingListLastImported);
        Assert.Equal(pldnsDate, saved.ImportStatus.PldnsListLastImported);
    }

    [Fact]
    public async Task CheckData_Get_SaveSessionThrows_DoesNotBubbleException_ReturnsView()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetJobRunsQueryResponse> { Success = true, Value = new GetJobRunsQueryResponse { JobRuns = new List<JobRun>() } });

        var controller = CreateControllerWithSession(CreateThrowingSessionOnSet());

        var result = await controller.CheckData();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
    }

    [Fact]
    public async Task CheckData_Post_InvalidModelState_ReturnsViewUsingSessionOrModel()
    {
        var session = CreateEmptySession();
        var sessionModel = new Rollover
        {
            ImportStatus = new RolloverImportStatus
            {
                RegulatedQualificationsLastImported = new DateTime(2025, 3, 3)
            }
        };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);
        controller.ModelState.AddModelError("any", "error");

        var posted = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = new DateTime(2024, 1, 1)
        };

        var result = await controller.CheckData(posted);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
        var vm = Assert.IsType<RolloverImportStatusViewModel>(viewResult.Model);
        Assert.Equal(sessionModel.ImportStatus.RegulatedQualificationsLastImported, vm.RegulatedQualificationsLastImported);
    }

    [Fact]
    public async Task CheckData_Post_ValidModel_WithSessionPreviousData_RedirectsToPreviousFile()
    {
        var session = CreateEmptySession();
        session.SetString("RolloverSession", JsonConvert.SerializeObject(new Rollover { PreviousData = new RolloverPreviousData { CandidateCount = 7 } }));
        var controller = CreateControllerWithSession(session);

        var posted = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = DateTime.UtcNow
        };

        var result = await controller.CheckData(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.PreviousFile), redirect.ActionName);
    }

    [Fact]
    public async Task CheckData_Post_ValidModel_WhenMediatorReturnsCandidates_SavesPreviousDataAndRedirects()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRolloverWorkflowCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>
            {
                Success = true,
                Value = new GetRolloverWorkflowCandidatesQueryResponse
                {
                    Data = new List<RolloverWorkflowCandidate>
                    {
                            new RolloverWorkflowCandidate()
                    }
                }
            });

        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var posted = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = DateTime.UtcNow
        };

        var result = await controller.CheckData(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.PreviousFile), redirect.ActionName);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(saved?.PreviousData);
        Assert.Equal(1, saved!.PreviousData!.CandidateCount);
    }

    [Fact]
    public async Task CheckData_Post_SavePreviousDataThrows_DoesNotBubbleException_RedirectsWhenCandidatesFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRolloverWorkflowCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>
            {
                Success = true,
                Value = new GetRolloverWorkflowCandidatesQueryResponse
                {
                    Data = new List<RolloverWorkflowCandidate> { new RolloverWorkflowCandidate() }
                }
            });

        var controller = CreateControllerWithSession(CreateThrowingSessionOnSet());

        var posted = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = DateTime.UtcNow
        };

        var result = await controller.CheckData(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.PreviousFile), redirect.ActionName);
    }

    [Fact]
    public async Task PreviousFile_Get_WhenSessionHasPreviousData_ReturnsViewWithSessionData()
    {
        var session = CreateEmptySession();
        var sessionModel = new Rollover
        {
            PreviousData = new RolloverPreviousData
            {
                CandidateCount = 10,
                SelectedOption = RolloverPreviousFileOption.ContinueProcessing
            }
        };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var result = await controller.PreviousFile();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("PreviousFile", viewResult.ViewName);
        var model = Assert.IsType<RolloverPreviousDataViewModel>(viewResult.Model);
        Assert.Equal(10, model.CandidateCount);
        Assert.Equal(sessionModel.PreviousData.SelectedOption, model.SelectedOption);
    }

    [Fact]
    public async Task PreviousFile_Get_WhenNoSession_CallsMediatorAndSavesSession()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetRolloverWorkflowCandidatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverWorkflowCandidatesQueryResponse>
            {
                Success = true,
                Value = new GetRolloverWorkflowCandidatesQueryResponse
                {
                    Data = new List<RolloverWorkflowCandidate>
                    {
                            new RolloverWorkflowCandidate
                            {
                                AcademicYear = "2024/25",
                                CreatedAt = DateTime.UtcNow.AddDays(-1),
                                CurrentFundingEndDate = DateTime.UtcNow.AddMonths(6),
                                FundingOfferId = Guid.NewGuid(),
                                Id = Guid.NewGuid(),
                                IncludedInFinalUpload = false,
                                IncludedInP1Export = false,
                                PassP1 = false,
                                ProposedFundingEndDate = DateTime.UtcNow.AddMonths(12),
                                QualificationVersionId = Guid.NewGuid(),
                                P1FailureReason = null,
                                RolloverCandidateRecordId = Guid.NewGuid(),
                                RolloverWorkflowRunId = Guid.NewGuid(),
                                UpdatedAt = DateTime.UtcNow
                            }
                    }
                }
            });

        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var result = await controller.PreviousFile();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("PreviousFile", viewResult.ViewName);
        var model = Assert.IsType<RolloverPreviousDataViewModel>(viewResult.Model);
        Assert.Equal(1, model.CandidateCount);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(saved?.PreviousData);
        Assert.Equal(1, saved!.PreviousData!.CandidateCount);
    }

    [Fact]
    public async Task PreviousFile_Post_InvalidModel_ReturnsView()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());
        controller.ModelState.AddModelError("x", "error");

        var model = new RolloverPreviousDataViewModel();

        var result = await controller.PreviousFile(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("PreviousFile", viewResult.ViewName);
        Assert.Same(model, viewResult.Model);
    }

    [Fact]
    public async Task PreviousFile_Post_ValidModel_ContinueProcessing_SavesSessionAndRedirectsToSelectFundingStreams()
    {
        var session = CreateEmptySession();
        session.SetString("RolloverSession", JsonConvert.SerializeObject(new Rollover { PreviousData = new RolloverPreviousData() }));
        var controller = CreateControllerWithSession(session);

        var model = new RolloverPreviousDataViewModel
        {
            SelectedOption = RolloverPreviousFileOption.ContinueProcessing
        };

        var result = await controller.PreviousFile(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.SelectFundingStreams), redirect.ActionName);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(saved?.PreviousData);
        Assert.Equal(model.SelectedOption, saved!.PreviousData!.SelectedOption);
    }

    [Fact]
    public async Task PreviousFile_Post_ValidModel_RemovePrevious_RedirectsToSelectCandidatesWithReturnAction()
    {
        var session = CreateEmptySession();
        session.SetString("RolloverSession", JsonConvert.SerializeObject(new Rollover { PreviousData = new RolloverPreviousData() }));
        var controller = CreateControllerWithSession(session);

        var model = new RolloverPreviousDataViewModel
        {
            SelectedOption = RolloverPreviousFileOption.RemovePrevious
        };

        var result = await controller.PreviousFile(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.SelectCandidates), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(nameof(RolloverController.PreviousFile), redirect.RouteValues["returnAction"]);
    }

    [Fact]
    public void SelectCandidates_Get_SetsTitle_AndReturnActionDefault()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());

        var result = controller.SelectCandidates();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("How do you want to select candidates for rollover", viewResult.ViewData["Title"]);
        Assert.Equal(nameof(RolloverController.CheckData), viewResult.ViewData["ReturnAction"]);
    }

    [Fact]
    public void SelectFundingStreams_Get_SetsTitle()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());

        var result = controller.SelectFundingStreams();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Select funding stream(s)", viewResult.ViewData["Title"]);
    }


    [Fact]
    public async Task UploadQualificationCandidates_ShouldRedirect_WhenValidMatchesFound()
    {
        // Arrange
        var uploadedModel = new RolloverUploadQualificationCandidatesViewModel
        {
            File = Mock.Of<IFormFile>()
        };

        var qan = "12345";

        var csvResult = new CsvFileReaderResult<QualificationCandidate>();
        csvResult.Items.Add(new QualificationCandidate { QualificationNumber = qan });

        _csvFileReaderMock
        .Setup(x => x.FileReadAsync(
            uploadedModel.File,
            QualificationImportColumns.Required,
            QualificationCandidateMapper.Map))
        .ReturnsAsync(csvResult);

        _mediatorMock
        .Setup(x => x.Send(It.IsAny<GetRolloverCandidatesQuery>(), default))
        .ReturnsAsync(new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>
        {
            Success = true,
            Value = new GetRolloverCandidatesQueryResponse
            {
                RolloverCandidates = new List<RolloverCandidate>
                {
                    new RolloverCandidate
                    {
                        Qan = "12345",
                        Title = "Test Qualification"
                    }
                }
            }
        });

        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        // Act
        var result = await controller.UploadQualificationCandidates(uploadedModel);

        // Assert
        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);

        Assert.NotNull(saved);
        Assert.NotNull(saved!.RolloverCandidates);
        Assert.NotEmpty(saved.RolloverCandidates);

        // Assert value matches
        var rollovercandidate = saved.RolloverCandidates.First();
        Assert.Equal(qan, rollovercandidate.QualificationNumber);
    }

    [Fact]
    public async Task UploadQualificationCandidates_ShouldRedirect_WhenFileNullAndSessionHasCandidates()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverCandidates = new List<QualificationCandidate>
            {
                new QualificationCandidate { QualificationNumber = "12345" }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new RolloverUploadQualificationCandidatesViewModel
        {
            File = null
        };

        // Act
        var result = await controller.UploadQualificationCandidates(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("FundingStreamInclusionExclusion", redirect.ActionName);
    }

    [Fact]
    public async Task UploadQualificationCandidates_ShouldReturnView_WhenModelStateInvalid()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverUploadQualificationCandidatesViewModel
        {
            File = Mock.Of<IFormFile>()
        };

        controller.ModelState.AddModelError("File", "Required");

        // Act
        var result = await controller.UploadQualificationCandidates(vm);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
    }

    [Fact]
    public async Task UploadQualificationCandidates_ShouldReturnView_WhenCsvIsInvalid()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverUploadQualificationCandidatesViewModel
        {
            File = Mock.Of<IFormFile>()
        };

        var csvResult = new CsvFileReaderResult<QualificationCandidate>
        {
            Errors = { "Invalid CSV", "Missing columns" }
        };

        _csvFileReaderMock
            .Setup(x => x.FileReadAsync(
                vm.File,
                QualificationImportColumns.Required,
                QualificationCandidateMapper.Map))
            .ReturnsAsync(csvResult);

        // Act
        var result = await controller.UploadQualificationCandidates(vm);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);

        var errors = controller.ModelState[nameof(vm.File)].Errors.Select(e => e.ErrorMessage);
        Assert.Contains("Invalid CSV", errors);
        Assert.Contains("Missing columns", errors);
    }

    [Fact]
    public async Task UploadQualificationCandidates_ShouldReturnView_WhenNoMatchedCandidatesFound()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverUploadQualificationCandidatesViewModel
        {
            File = Mock.Of<IFormFile>()
        };

        var csvResult = new CsvFileReaderResult<QualificationCandidate>
        {
            Items = { new QualificationCandidate { QualificationNumber = "11111" } }
        };

        _csvFileReaderMock
            .Setup(x => x.FileReadAsync(
                vm.File,
                QualificationImportColumns.Required,
                QualificationCandidateMapper.Map))
            .ReturnsAsync(csvResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetRolloverCandidatesQuery>(), default))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>
            {
                Success = true,
                Value = new GetRolloverCandidatesQueryResponse
                {
                    RolloverCandidates = new List<RolloverCandidate>() // no matches
                }
            });

        // Act
        var result = await controller.UploadQualificationCandidates(vm);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(vm.File)));

        var msg = controller.ModelState[nameof(vm.File)]
            .Errors.First().ErrorMessage;

        Assert.Equal("No valid candidates found.", msg);
    }

    [Fact]
    public async Task UploadQualificationCandidates_ShouldReturnView_WhenMediatorReturnsNull()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var vm = new RolloverUploadQualificationCandidatesViewModel
        {
            File = Mock.Of<IFormFile>()
        };

        var csvResult = new CsvFileReaderResult<QualificationCandidate>
        {
            Items = { new QualificationCandidate { QualificationNumber = "12345" } }
        };

        _csvFileReaderMock
            .Setup(x => x.FileReadAsync(
                vm.File,
                QualificationImportColumns.Required,
                QualificationCandidateMapper.Map))
            .ReturnsAsync(csvResult);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetRolloverCandidatesQuery>(), default))
            .ReturnsAsync((BaseMediatrResponse<GetRolloverCandidatesQueryResponse>)null!);

        // Act
        var result = await controller.UploadQualificationCandidates(vm);

        // Assert: no crash, fallback behaviour = no matches
        var view = Assert.IsType<ViewResult>(result);
        Assert.True(controller.ModelState.ContainsKey(nameof(vm.File)));
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_WhenSessionExists_ReturnsViewWithModel()
    {
        // Arrange
        var session = CreateEmptySession();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream> { new() { Id = "FS1" } },
                SelectedIds = new List<string> { "FS1" }
            }
        };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        // Assert
        var result = await controller.FundingStreamInclusionExclusion();
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        Assert.NotNull(result);
        Assert.Equal("FS1", vm.FundingStreams.First().Id);
        Assert.Contains("FS1", vm.SelectedIds);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_NoSession_BuildsFundingStreamsAndSaves()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverCandidates = new List<QualificationCandidate>
            {
                new QualificationCandidate
                {
                    FundingOfferId = "FS1"
                }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        // Act
        var result = await controller.FundingStreamInclusionExclusion();
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        // Assert
        Assert.NotNull(result);
        Assert.Single(vm.FundingStreams);
        Assert.Equal("FS1", vm.FundingStreams.First().Id);

        // Session should now contain RolloverFundingStream
        var updatedSessionJson = session.GetString("RolloverSession");
        var updatedSession = JsonConvert.DeserializeObject<Rollover>(updatedSessionJson);

        Assert.NotNull(updatedSession.RolloverFundingStream);
        Assert.Single(updatedSession.RolloverFundingStream.FundingStreams);
        Assert.Equal("FS1", updatedSession.RolloverFundingStream.FundingStreams.First().Id);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_NoFundingStreams_AddsModelError()
    {
        // Arrange
        var session = CreateEmptySession();

        // RolloverCandidates exists but leads to zero funding streams
        var sessionModel = new Rollover
        {
            RolloverCandidates = new List<QualificationCandidate>()
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        // Act
        var result = await controller.FundingStreamInclusionExclusion();
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        // Assert
        Assert.True(!controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(vm.FundingStreams)));

        var error = controller.ModelState[nameof(vm.FundingStreams)]
            .Errors
            .First()
            .ErrorMessage;

        Assert.Equal("No Funding Streams found.", error);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_SelectAll_ReturnsViewWithAllIdsSelected()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
                {
                    new FundingStream { Id = "FS1" },
                    new FundingStream { Id = "FS2" }
                },
                SelectedIds = new List<string>()
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<string>()
        };

        // Act
        var result = await controller.FundingStreamInclusionExclusion(vm, "selectAll");
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedVm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        // Assert
        Assert.Equal(2, returnedVm.SelectedIds.Count);
        Assert.Contains("FS1", returnedVm.SelectedIds);
        Assert.Contains("FS2", returnedVm.SelectedIds);

        Assert.Empty(controller.ModelState); // ModelState cleared
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_NoSelection_ReturnsModelError()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
                {
                    new FundingStream { Id = "FS1" }
                }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = null
        };

        // Act
        var result = await controller.FundingStreamInclusionExclusion(vm, action: "");
        var viewResult = Assert.IsType<ViewResult>(result);

        // Assert
        Assert.True(!controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(vm.SelectedIds)));

        var error = controller.ModelState[nameof(vm.SelectedIds)].Errors.First().ErrorMessage;
        Assert.Equal("Select at least one funding stream.", error);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_InvalidSelection_ReturnsError()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
                {
                    new FundingStream { Id = "FS1" }
                }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<string> { "INVALID" } // not in FS1
        };

        // Act
        var result = await controller.FundingStreamInclusionExclusion(vm, action: "");
        var viewResult = Assert.IsType<ViewResult>(result);

        // Assert
        Assert.True(!controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(""));

        var error = controller.ModelState[""].Errors.First().ErrorMessage;
        Assert.Equal("Invalid selection", error);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_ValidSelection_SavesSessionAndRedirects()
    {
        // Arrange
        var session = CreateEmptySession();

        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
            {
                new FundingStream { Id = "FS1" }
            }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<string> { "FS1" },
            FundingStreams = sessionModel.RolloverFundingStream.FundingStreams
        };

        // Act
        var result = await controller.FundingStreamInclusionExclusion(vm, action: "");
        var redirect = Assert.IsType<RedirectToActionResult>(result);

        // Assert redirect
        Assert.Equal(nameof(controller.EnterRolloverEligibilityDates), redirect.ActionName);

        // Assert session updated
        var updatedJson = session.GetString("RolloverSession");
        var updated = JsonConvert.DeserializeObject<Rollover>(updatedJson);

        Assert.Equal("FS1", updated.RolloverFundingStream.SelectedIds.First());
    }

    [Fact]
    public async Task EnterRolloverEligibilityDates_ReturnsView()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        // Act
        var result = await controller.EnterRolloverEligibilityDates();
        var viewResult = Assert.IsType<ViewResult>(result);

        // Assert
        Assert.Null(viewResult.ViewName); // default view
    }

    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    private class ThrowingSession : ISession
    {
        private readonly bool _throwOnGet;
        private readonly bool _throwOnSet;

        public ThrowingSession(bool throwOnGet, bool throwOnSet)
        {
            _throwOnGet = throwOnGet;
            _throwOnSet = throwOnSet;
        }

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => Array.Empty<string>();

        public void Clear()
        {
            if (_throwOnSet) throw new Exception("Clear failed");
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key)
        {
            if (_throwOnSet) throw new Exception("Remove failed");
        }

        public void Set(string key, byte[] value)
        {
            if (_throwOnSet) throw new Exception("Set failed");
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_throwOnGet) throw new Exception("Get failed");
            value = Array.Empty<byte>();
            return false;
        }
    }
}