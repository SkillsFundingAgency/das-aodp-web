using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class RolloverControllerTests
{
    private readonly Mock<ILogger<RolloverController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RolloverController _controller;
    private readonly Mock<IValidator<RolloverEligibilityDatesViewModel>> _validatorMock;

    public RolloverControllerTests()
    {
        _loggerMock = new Mock<ILogger<RolloverController>>();
        _mediatorMock = new Mock<IMediator>();
        _validatorMock = new Mock<IValidator<RolloverEligibilityDatesViewModel>>();
        _controller = new RolloverController(_loggerMock.Object, _mediatorMock.Object, _validatorMock.Object);
    }

    private RolloverController CreateControllerWithSession(ISession session)
    {
        var controller = new RolloverController(_loggerMock.Object, _mediatorMock.Object, _validatorMock.Object);
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

        // should not throw, should return view result
        var result = controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
    }

    [Fact]
    public void Index_Post_InvalidModelState_ReturnsStartView_WithSameModel()
    {
        var controller = CreateControllerWithSession(CreateEmptySession());
        controller.ModelState.AddModelError("SelectedProcess", "required");

        var vm = new RolloverStartViewModel();

        var result = controller.Index(vm);

        // Assert
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
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);

        // session should contain Start with SelectedProcess
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

        // should not throw despite session.Set throwing internally in SaveSessionModel
        var result = controller.Index(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);
    }

    [Fact]
    public void InitialSelection_Get_SetsTitle_And_UsesSessionImportStatusWhenPresent()
    {
        var session = CreateEmptySession();

        // prepare a session model with ImportStatus (session DTO)
        var sessionModel = new Rollover
        {
            ImportStatus = new RolloverImportStatus
            {
                RegulatedQualificationsLastImported = new DateTime(2025, 1, 1)
            }
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(sessionModel));

        var controller = CreateControllerWithSession(session);

        var result = controller.InitialSelection();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Initial selection of qualificaton", viewResult.ViewData["Title"]);
        // ImportStatus should be present in ViewData
        Assert.NotNull(viewResult.ViewData["ImportStatus"]);
        var importStatus = Assert.IsType<RolloverImportStatus>(viewResult.ViewData["ImportStatus"]);
        Assert.Equal(sessionModel.ImportStatus.RegulatedQualificationsLastImported, importStatus.RegulatedQualificationsLastImported);
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
        // arrange mediator to return a single job run per job name with specific EndTime
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

        // session should have been saved with same values
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
    public void CheckData_Post_InvalidModelState_ReturnsViewUsingSessionOrModel()
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

        var result = controller.CheckData(posted);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
        var vm = Assert.IsType<RolloverImportStatusViewModel>(viewResult.Model);
        Assert.Equal(sessionModel.ImportStatus.RegulatedQualificationsLastImported, vm.RegulatedQualificationsLastImported);
    }

    [Fact]
    public void CheckData_Post_ValidModel_SavesSessionAndRedirects()
    {
        var session = CreateEmptySession();
        var controller = CreateControllerWithSession(session);

        var posted = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = new DateTime(2026, 4, 4)
        };

        var result = controller.CheckData(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);

        var json = session.GetString("RolloverSession");
        Assert.NotNull(json);
        var saved = JsonConvert.DeserializeObject<Rollover>(json!);
        Assert.NotNull(saved?.ImportStatus);
        Assert.Equal(posted.RegulatedQualificationsLastImported, saved!.ImportStatus!.RegulatedQualificationsLastImported);
    }

    [Fact]
    public void EnterRolloverEligibilityDates_Get_ReturnsViewAndSetsTitle()
    {
        // Act
        var result = _controller.EnterRolloverEligibilityDates();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Enter rollover eligibility dates", viewResult.ViewData["Title"]);
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