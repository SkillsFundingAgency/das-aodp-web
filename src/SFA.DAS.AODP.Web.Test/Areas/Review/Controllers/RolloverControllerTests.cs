using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class RolloverControllerTests
{
    private readonly Mock<ILogger<RolloverController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RolloverController _controller;
    private readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> _eligibilityDatesValidatorMock;
    private readonly Mock<IValidator<RolloverFundingApprovalEndDateViewModel>> _approvalEndDateValidatorMock;
    private readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> _validatorMock;
    private readonly Mock<ICsvFileReader> _csvFileReaderMock;
    private readonly Mock<IUserHelperService> _userHelperServiceMock;

    public RolloverControllerTests()
    {
        _loggerMock = new Mock<ILogger<RolloverController>>();
        _mediatorMock = new Mock<IMediator>();
        _validatorMock = new Mock<IValidator<RolloverEligibilityDatesViewModel>>();
        _csvFileReaderMock = new Mock<ICsvFileReader>();
        _eligibilityDatesValidatorMock = new Mock<IValidator<RolloverEligibilityDatesViewModel>>();
        _approvalEndDateValidatorMock = new Mock<IValidator<RolloverFundingApprovalEndDateViewModel>>();
        _userHelperServiceMock = new Mock<IUserHelperService>();
        _controller = new RolloverController(_loggerMock.Object,
            _mediatorMock.Object,
            _eligibilityDatesValidatorMock.Object,
            _approvalEndDateValidatorMock.Object,
            _csvFileReaderMock.Object,
            _userHelperServiceMock.Object);
    }

    private RolloverController CreateControllerWithSession(ISession session)
    {
        var controller = new RolloverController(_loggerMock.Object,
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
        return controller;
    }

    private static ISession CreateEmptySession() => new TestSession();
    private static ISession CreateThrowingSessionOnGet() => new ThrowingSession(throwOnGet: true, throwOnSet: false);
    private static ISession CreateThrowingSessionOnSet() => new ThrowingSession(throwOnGet: false, throwOnSet: true);

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

        // Assert
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
            .Setup(m => m.Send(It.IsAny<GetRolloverWorkflowCandidatesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>
            {
                Success = true,
                Value = new GetRolloverWorkflowCandidatesCountQueryResponse { TotalRecords = 5 }
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
        Assert.Equal(5, saved!.PreviousData!.CandidateCount);
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
            .Setup(m => m.Send(It.IsAny<GetRolloverWorkflowCandidatesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>
            {
                Success = true,
                Value = new GetRolloverWorkflowCandidatesCountQueryResponse { TotalRecords = 1 }
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
        var model = viewResult.Model as RolloverSelectCandidatesViewModel;
        Assert.Equal(nameof(RolloverController.CheckData), model.ReturnUrl);
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
    public void SelectCandidates_Get_WhenSessionHasSelectCandidates_PopulatesModel()
    {
        // arrange
        var session = new TestSession();
        var saved = new Rollover
        {
            SelectCandidates = new RolloverSelectCandidates
            {
                SelectedOption = SelectCandidatesForRollover.GenerateAList,
                ReturnUrl = "SavedReturn"
            }
        };
        session.Set("RolloverSession", System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(saved)));

        var controller = CreateControllerWithSession(session);

        // act
        var result = controller.SelectCandidates();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SelectCandidates", viewResult.ViewName);
        var model = Assert.IsType<RolloverSelectCandidatesViewModel>(viewResult.Model);
        Assert.Equal(SelectCandidatesForRollover.GenerateAList, model.SelectedOption);
        Assert.Equal("SavedReturn", model.ReturnUrl);
    }

    [Fact]
    public void SelectCandidates_Post_InvalidModelState_ReturnsViewAndSetsTitle()
    {
        // arrange
        var controller = CreateControllerWithSession(new TestSession());
        controller.ModelState.AddModelError("SelectedOption", "required");

        var posted = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = null,
            ReturnUrl = "someReturn"
        };

        // act
        var result = controller.SelectCandidates(posted);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SelectCandidates", viewResult.ViewName);
        Assert.Same(posted, viewResult.Model);
    }

    [Fact]
    public void SelectCandidates_Post_ValidModel_ImportAList_SavesSessionAndRedirects()
    {
        // arrange
        var session = new TestSession();
        var controller = CreateControllerWithSession(session);

        var posted = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = SelectCandidatesForRollover.ImportAList,
            ReturnUrl = "return123"
        };

        // act
        var result = controller.SelectCandidates(posted);

        // assert redirect
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.ImportCandidatesList), redirect.ActionName);

        // assert
        Assert.True(session.TryGetValue("RolloverSession", out var bytes));
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        var saved = JsonConvert.DeserializeObject<Rollover>(json);
        Assert.NotNull(saved);
        Assert.NotNull(saved.SelectCandidates);
        Assert.Equal(SelectCandidatesForRollover.ImportAList, saved.SelectCandidates.SelectedOption);
        Assert.Equal("return123", saved.SelectCandidates.ReturnUrl);
    }

    [Fact]
    public void SelectCandidates_Post_ValidModel_GenerateAList_SavesSessionAndRedirects()
    {
        // arrange
        var session = new TestSession();
        var controller = CreateControllerWithSession(session);

        var posted = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = SelectCandidatesForRollover.GenerateAList,
            ReturnUrl = "r2"
        };

        // act
        var result = controller.SelectCandidates(posted);

        // assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.RolloverQueryBuilder), redirect.ActionName);

        Assert.True(session.TryGetValue("RolloverSession", out var bytes));
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        var saved = JsonConvert.DeserializeObject<Rollover>(json);
        Assert.NotNull(saved);
        Assert.NotNull(saved.SelectCandidates);
        Assert.Equal(SelectCandidatesForRollover.GenerateAList, saved.SelectCandidates.SelectedOption);
        Assert.Equal("r2", saved.SelectCandidates.ReturnUrl);
    }

    [Fact]
    public void ImportCandidatesList_Get_SetsTitle()
    {
        // arrange
        var controller = CreateControllerWithSession(new TestSession());

        // act
        var result = controller.ImportCandidatesList();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Import Candidates List ", viewResult.ViewData["Title"]);
    }

    [Fact]
    public void RolloverQueryBuilder_Get_SetsTitle()
    {
        // arrange
        var controller = CreateControllerWithSession(new TestSession());

        // act
        var result = controller.RolloverQueryBuilder();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Rollover Query Builder", viewResult.ViewData["Title"]);
    }

    [Fact]
    public async Task EnterRolloverEligibilityDates_Get_ReturnsViewAndSetsTitleAsync()
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
                        QualificationNumber = "12345"
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
        var id = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream> { new() { Id = id } },
                SelectedIds = new List<Guid> { id }
            }
        };
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        // Assert
        var result = await controller.FundingStreamInclusionExclusion();
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        Assert.NotNull(result);
        Assert.Equal(id, vm.FundingStreams.First().Id);
        Assert.Contains(id, vm.SelectedIds);
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_NoSession_BuildsFundingStreamsAndSaves()
    {
        // Arrange
        var session = CreateEmptySession();
        var id = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverCandidates = new List<QualificationCandidate>
            {
                new QualificationCandidate
                {
                    FundingOfferId = id
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
        Assert.Equal(id, vm.FundingStreams.First().Id);

        // Session should now contain RolloverFundingStream
        var updatedSessionJson = session.GetString("RolloverSession");
        var updatedSession = JsonConvert.DeserializeObject<Rollover>(updatedSessionJson);

        Assert.NotNull(updatedSession.RolloverFundingStream);
        Assert.Single(updatedSession.RolloverFundingStream.FundingStreams);
        Assert.Equal(id, updatedSession.RolloverFundingStream.FundingStreams.First().Id);
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
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
            {
                new FundingStream { Id = id1 },
                new FundingStream { Id = id2 }
            },
                SelectedIds = new List<Guid>()
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<Guid>()
        };

        // Act
        var result = await controller.FundingStreamInclusionExclusion(vm, "selectAll");
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedVm = Assert.IsType<FundingStreamInclusionExclusionViewModel>(viewResult.Model);

        // Assert
        Assert.Equal(2, returnedVm.SelectedIds.Count);
        Assert.Contains(id1, returnedVm.SelectedIds);
        Assert.Contains(id2, returnedVm.SelectedIds);

        Assert.Empty(controller.ModelState); // ModelState cleared
    }

    [Fact]
    public async Task FundingStreamInclusionExclusion_NoSelection_ReturnsModelError()
    {
        // Arrange
        var session = CreateEmptySession();
        var id = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
            {
                new FundingStream { Id = id }
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
        var id = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
            {
                new FundingStream { Id = id }
            }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<Guid> { Guid.NewGuid() } // not in FS1
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
        var id = Guid.NewGuid();
        var sessionModel = new Rollover
        {
            RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = new List<FundingStream>
                {
                    new FundingStream { Id = id}
                }
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            SelectedIds = new List<Guid> { id },
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

        Assert.Equal(id, updated.RolloverFundingStream.SelectedIds.First());
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

    [Fact]
    public void EnterRolloverFundingApprovalEndDate_Get_ReturnsView()
    {
        // Arrange
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        // Act
        var result = _controller.EnterRolloverFundingApprovalEndDate();
        var viewResult = Assert.IsType<ViewResult>(result);

        // Assert
        Assert.Null(viewResult.ViewName); // default view
    }

    [Fact]
    public async Task EnterRolloverFundingApprovalEndDate_ReturnsView_WhenModelStateInvalid()
    {
        // Arrange
        var model = new RolloverFundingApprovalEndDateViewModel();

        _approvalEndDateValidatorMock
            .Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("MaxApprovalEndDate", "Required")
            }));

        var session = new TestSession();
        var controller = CreateControllerWithSession(session);

        // Act
        var result = await controller.EnterRolloverFundingApprovalEndDate(model);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("EnterRolloverFundingApprovalEndDate", view.ViewName);
        Assert.Equal(model, view.Model);
    }

    [Fact]
    public async Task EnterRolloverFundingApprovalEndDate_SavesSession_AndSendsCommand_OnValidModel()
    {
        var date = new DateOnly(2028, 3, 15);

        // Arrange
        var model = new RolloverFundingApprovalEndDateViewModel
        {
            MaxApprovalEndDate = new RolloverFundingApprovalEndDate { Day = date.Day, Month = date.Month, Year = date.Year }
        };

        _approvalEndDateValidatorMock
            .Setup(v => v.ValidateAsync(model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var candidateId = Guid.NewGuid();
        var fundingOfferId = Guid.NewGuid();

        var sessionModel = new Rollover
        {
            RolloverCandidates = new List<QualificationCandidate>
            {
                new QualificationCandidate
                {
                    AcademicYear = "2024/25",
                    RolloverCandidateId = candidateId
                }
            },
            RolloverEligibilityDates = new RolloverEligibilityDates
            {
                FundingEndDate = new RolloverEligibilityDate { Day = date.Day, Month = date.Month, Year = date.Year },
                OperationalEndDate = new RolloverEligibilityDate { Day = date.Day, Month = date.Month, Year = date.Year }
            },
            RolloverFundingStream = new RolloverFundingStream
            {
                SelectedIds = new List<Guid> { fundingOfferId }
            }
        };

        // Put session model into the test session
        var session = CreateEmptySession();
        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        // Track what command was sent to mediator
        CreateRolloverWorkflowRunCommand? sentCommand = null;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateRolloverWorkflowRunCommand>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((cmd, _) => sentCommand = (CreateRolloverWorkflowRunCommand)cmd)
            .ReturnsAsync(new BaseMediatrResponse<CreateRolloverWorkflowRunCommandResponse>
            {
                Success = true,
                Value = new CreateRolloverWorkflowRunCommandResponse { RolloverWorkflowRunId = Guid.NewGuid() }
            });

        _userHelperServiceMock
            .Setup(s => s.GetUserDisplayName())
            .Returns("Test User");

        // Act
        var result = await controller.EnterRolloverFundingApprovalEndDate(model);

        // Assert session updated
        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var updatedSession = JsonConvert.DeserializeObject<Rollover>(json!);

        Assert.NotNull(updatedSession!.RolloverFundingApprovalEndDate);
        Assert.Equal(15, updatedSession.RolloverFundingApprovalEndDate.Day);
        Assert.Equal(3, updatedSession.RolloverFundingApprovalEndDate.Month);
        Assert.Equal(2028, updatedSession.RolloverFundingApprovalEndDate.Year);

        // Assert mediator command was sent
        Assert.NotNull(sentCommand);
        Assert.Equal("2024/25", sentCommand!.AcademicYear);
        Assert.Contains(candidateId, sentCommand.RolloverCandidateIds);
        Assert.Contains(fundingOfferId, sentCommand.FundingOfferIds);
        Assert.Equal("Test User", sentCommand.CreatedByUserName);
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