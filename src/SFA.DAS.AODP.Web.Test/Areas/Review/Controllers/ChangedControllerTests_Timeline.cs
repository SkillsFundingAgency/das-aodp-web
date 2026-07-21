using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.UnitTests.Helper;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerTests_Timeline
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ChangedController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelper;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ChangedController _controller;

    public ChangedControllerTests_Timeline()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());

        _loggerMock = _fixture.Freeze<Mock<ILogger<ChangedController>>>();
        _userHelper = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _controller = new ChangedController(_loggerMock.Object, _mediatorMock.Object, _userHelper.Object);

        var context = new DefaultHttpContext();
        context.Session = new TestSession();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        _controller.TempData = new TempDataDictionary(_controller.HttpContext, Mock.Of<ITempDataProvider>());

        _userHelper
            .Setup(u => u.GetUserRoles())
            .Returns(new List<string>());

        var processResponse = new BaseMediatrResponse<GetProcessStatusesQueryResponse>
        {
            Success = true,
            Value = new GetProcessStatusesQueryResponse
            {
                ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>
                {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" },
                    new() { Id = Guid.NewGuid(), Name = "No Action Required" }
                }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processResponse);
    }

    [Fact]
    public async Task Timeline_SetsQan()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetDiscussionHistoriesForQualificationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = new GetDiscussionHistoriesForQualificationQueryResponse
                {
                    QualificationDiscussionHistories = new()
                }
            });

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetQualificationDetailWithVersionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = new GetQualificationDetailsQueryResponse
                {
                    Qual = new GetQualificationDetailsQueryResponse.Qualification
                    {
                        Qan = "ABC",
                        Versions = new()
                        {
                        new GetQualificationDetailsQueryResponse
                        {
                            Version = 1,
                            InsertedTimestamp = DateTime.UtcNow,
                            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Status" }
                        }
                        }
                    },
                    Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                    Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                    ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Status" }
                }
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Equal("ABC", model.Qan);
    }

    [Fact]
    public async Task Timeline_AddsChangeHistoryEntries()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetDiscussionHistoriesForQualificationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = new GetDiscussionHistoriesForQualificationQueryResponse
                {
                    QualificationDiscussionHistories = new()
                }
            });

        var versions = new List<GetQualificationDetailsQueryResponse>
        {
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 2,
                InsertedTimestamp = DateTime.UtcNow,
                Level = "L2",
                VersionFieldChanges = "Level",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            },
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 1,
                InsertedTimestamp = DateTime.UtcNow.AddMinutes(-10),
                Level = "L1",
                VersionFieldChanges = "Level",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetQualificationDetailWithVersionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = new GetQualificationDetailsQueryResponse
                {
                    Qual = new GetQualificationDetailsQueryResponse.Qualification
                    {
                        Id = Guid.NewGuid(),
                        Qan = "ABC",
                        QualificationName = "Test",
                        Versions = versions
                    },
                    Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                    Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                    ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
                }
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.NotEmpty(model.QualificationDiscussionHistories);
        Assert.Equal("Change", model.QualificationDiscussionHistories[0].Title);
        Assert.Equal("OFQUAL Import", model.QualificationDiscussionHistories[0].UserDisplayName);
    }


    [Fact]
    public async Task Timeline_MultipleChanges()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetDiscussionHistoriesForQualificationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = new GetDiscussionHistoriesForQualificationQueryResponse
                {
                    QualificationDiscussionHistories = new()
                }
            });

        var versions = new List<GetQualificationDetailsQueryResponse>
        {
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 3,
                InsertedTimestamp = DateTime.UtcNow,
                Level = "L3",
                Status = "NewStatus",
                VersionFieldChanges = "Level,Status",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            },
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 2,
                InsertedTimestamp = DateTime.UtcNow.AddMinutes(-10),
                Level = "L2",
                Status = "MidStatus",
                VersionFieldChanges = "Level,Status",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            },
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 1,
                InsertedTimestamp = DateTime.UtcNow.AddMinutes(-20),
                Level = "L1",
                Status = "OldStatus",
                VersionFieldChanges = "Level,Status",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetQualificationDetailWithVersionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = new GetQualificationDetailsQueryResponse
                {
                    Qual = new GetQualificationDetailsQueryResponse.Qualification
                    {
                        Id = Guid.NewGuid(),
                        Qan = "ABC",
                        QualificationName = "Test",
                        Versions = versions
                    },
                    Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                    Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                    ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
                }
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.True(model.QualificationDiscussionHistories.Count >= 2);
        Assert.All(model.QualificationDiscussionHistories, h => Assert.Equal("Change", h.Title));
    }

    [Fact]
    public async Task Timeline_NoChanges()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetDiscussionHistoriesForQualificationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = new GetDiscussionHistoriesForQualificationQueryResponse
                {
                    QualificationDiscussionHistories = new()
                }
            });

        var versions = new List<GetQualificationDetailsQueryResponse>
        {
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 2,
                InsertedTimestamp = DateTime.UtcNow,
                Level = "L1",
                Status = "SameStatus",
                VersionFieldChanges = "",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            },
            new GetQualificationDetailsQueryResponse
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                Version = 1,
                InsertedTimestamp = DateTime.UtcNow.AddMinutes(-10),
                Level = "L1",
                Status = "SameStatus",
                VersionFieldChanges = "",
                Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetQualificationDetailWithVersionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = new GetQualificationDetailsQueryResponse
                {
                    Qual = new GetQualificationDetailsQueryResponse.Qualification
                    {
                        Id = Guid.NewGuid(),
                        Qan = "ABC",
                        QualificationName = "Test",
                        Versions = versions
                    },
                    Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Stage" },
                    Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Org" },
                    ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Proc" }
                }
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Empty(model.QualificationDiscussionHistories);
    }


    [Fact]
    public async Task Timeline_ReturnsErrorOnFailure()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<GetDiscussionHistoriesForQualificationQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = false,
                ErrorMessage = "Boom"
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        Assert.IsType<RedirectResult>(result);
    }
}
