using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.UnitTests.Helper;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Models.Session;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerTests:UnitTest
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ChangedController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelper;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ChangedController _controller;

    public ChangedControllerTests()
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
    public async Task Index_ReturnsViewResult_Empty()
    {
        // Arrange: Changed qualifications response
        var changedResponse = new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
        {
            Success = true,
            Value = new GetChangedQualificationsQueryResponse
            {
                Data = _fixture.CreateMany<ChangedQualification>(2).ToList(),
                TotalRecords = 2,
                Skip = 0,
                Take = 10
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(changedResponse);

        var statusesResponse = new BaseMediatrResponse<GetProcessStatusesQueryResponse>
        {
            Success = true,
            Value = new GetProcessStatusesQueryResponse
            {
                ProcessStatuses =
                {
                    new GetProcessStatusesQueryResponse.ProcessStatus
                    {
                        Id = Guid.NewGuid(),
                        Name = "Decision Required"
                    }
                }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusesResponse);

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        var json = System.Text.Json.JsonSerializer.Serialize(sessionModel);
        _controller.HttpContext.Session.SetString("ChangedQualificationFilters", json);

        // Act
        var result = await _controller.Index(pageNumber: 1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);
    }


    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();
        queryResponse.Value.TotalRecords = 2;
        queryResponse.Value.Take = 10;
        queryResponse.Value.Skip = 0;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(queryResponse);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
                     {
                         Success = true,
                         Value = new GetProcessStatusesQueryResponse()
                     });

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        var json = System.Text.Json.JsonSerializer.Serialize(sessionModel);
        _controller.HttpContext.Session.SetString("ChangedQualificationFilters", json);

        // Act
        var result = await _controller.Index(pageNumber: 1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);

        Assert.Equal(2, model.ChangedQualifications.Count);
        Assert.Equal(queryResponse.Value.Data[0].Subject, model.ChangedQualifications[0].Subject);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.ChangedQualifications[0].Status);
        Assert.Equal(queryResponse.Value.Data[0].AwardingOrganisation, model.ChangedQualifications[0].AwardingOrganisationName);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.ChangedQualifications[0].Status);
    }


    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(queryResponse);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
                     {
                         Success = true,
                         Value = new GetProcessStatusesQueryResponse()
                     });

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = 1,
            RecordsPerPage = 10,
            Organisation = null,
            QualificationName = null,
            QAN = null,
            ProcessStatusIds = new List<Guid>(),
            AgeGroups = new List<AgeGroup>()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(sessionModel);
        _controller.HttpContext.Session.SetString("ChangedQualificationFilters", json);

        // Act
        var result = await _controller.Index(pageNumber: 1);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Index_StoresUpdatedSessionModel()
    {
        // Arrange
        var sessionModel = new QualificationFilterSessionModel { PageNumber = 1, RecordsPerPage = 10 };
        _controller.HttpContext.Session.SetObject("ChangedQualificationFilters", sessionModel);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse { Data = new(), TotalRecords = 0 }
            });

        // Act
        await _controller.Index(pageNumber: 5);

        // Assert
        var updated = _controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");
        Assert.Equal(5, updated.PageNumber);
    }

    [Fact]
    public async Task Index_InvalidPaging_ShowsNotification()
    {
        // Arrange
        var sessionModel = new QualificationFilterSessionModel { PageNumber = -1, RecordsPerPage = 999 };
        _controller.HttpContext.Session.SetObject("ChangedQualificationFilters", sessionModel);

        _controller.TempData[ChangedController.NewQualDataKeys.InvalidPageParams.ToString()] = true;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse { Data = new(), TotalRecords = 0 }
            });

        // Act
        var result = await _controller.Index(pageNumber: -1);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(_controller.TempData[ChangedController.NewQualDataKeys.InvalidPageParams.ToString()]);
    }


    [Fact]
    public async Task Index_Exception_RedirectsToError()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _controller.Index(pageNumber:1);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }


    [Fact]
    public async Task QualificationDetails_Get_ReturnsViewWithModel()
    {
        // Arrange
        var details = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Version = 1,
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = "ABC123",
                QualificationName = "Test Qualification",
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = "Test Org"
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Draft"
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = "Decision Required"
            }
        };

        var wrappedDetails = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
        {
            Success = true,
            Value = details
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrappedDetails);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = new GetFeedbackForQualificationFundingByIdQueryResponse
                {
                    QualificationFundedOffers = new()
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = new GetApplicationsByQanQueryResponse
                {
                    Applications = new()
                }
            });

        // Act
        var result = await _controller.QualificationDetails("ABC123");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);
    }

    [Fact]
    public async Task QualificationDetails_Get_LoadsPreviousVersion_AndPopulatesKeyFieldChanges()
    {
        // Arrange latest version
        var latest = _fixture.Create<GetQualificationDetailsQueryResponse>();
        latest.Version = 2;
        latest.VersionFieldChanges = "Title,OrganisationName";

        var latestWrapped = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
        {
            Success = true,
            Value = latest
        };

        // Arrange previous version
        var previous = _fixture.Create<GetQualificationDetailsQueryResponse>();
        previous.Version = 1;

        var previousWrapped = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
        {
            Success = true,
            Value = previous
        };

        // Mock required mediator calls
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestWrapped);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationVersionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousWrapped);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" }
                    }
                }
            });

        // REQUIRED: feedback call
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = new GetFeedbackForQualificationFundingByIdQueryResponse
                {
                    QualificationFundedOffers = new()
                }
            });

        // REQUIRED: applications call
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = new GetApplicationsByQanQueryResponse
                {
                    Applications = new()
                }
            });

        // Act
        var result = await _controller.QualificationDetails("ABC123");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);

        Assert.Equal(2, model.Version);
        Assert.NotEmpty(model.KeyFieldChanges);

        Assert.Contains(model.KeyFieldChanges, k => k.Name.Contains("Title", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(model.KeyFieldChanges, k => k.Name.Contains("Organisation", StringComparison.OrdinalIgnoreCase));
    }


    [Fact(Skip = "This test is being ignored for now.")]
    public async Task QualificationDetails_ReturnsViewResult_WithQualificationDetails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationDetailsViewModel>(viewResult.ViewData.Model);
    }

    [Fact(Skip = "This test is being ignored for now.")]
    [ExcludeFromCodeCoverage]
    public async Task QualificationDetails_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = false;
        queryResponse.ErrorMessage = "No details found for qualification reference: Ref123";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        try
        {
            var result = await _controller.QualificationDetails("Ref123");
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.Equal(queryResponse.ErrorMessage, ex.Message);
        }
    }

    [Fact]
    public async Task QualificationDetails_ReturnsBadRequest_WhenQualificationReferenceIsEmpty()
    {
        // Act
        var result = await _controller.QualificationDetails(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<RedirectResult>(result);
    }

    [Fact]
    public async Task QualificationDetails_Get_WhenVersionGreaterThanOne_LoadsPreviousVersion_AndPopulatesKeyFieldChanges()
    {
        // Arrange
        var latest = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Version = 2,
            VersionFieldChanges = "Title,OrganisationName",
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = "ABC123",
                QualificationName = "New Name",
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = "New Org"
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Draft"
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = "Decision Required"
            }
        };

        var previous = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = latest.QualificationId,
            Version = 1,
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = "ABC123",
                QualificationName = "Old Name",
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = "Old Org"
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Draft"
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = "Decision Required"
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = latest
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationVersionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = previous
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = new GetFeedbackForQualificationFundingByIdQueryResponse
                {
                    QualificationFundedOffers = new()
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = new GetApplicationsByQanQueryResponse
                {
                    Applications = new()
                }
            });

        // Act
        var result = await _controller.QualificationDetails("ABC123");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);
        Assert.Equal(2, model.Version);
        Assert.NotEmpty(model.KeyFieldChanges);
        Assert.Contains(model.KeyFieldChanges, k => k.Name.Contains("Title", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(model.KeyFieldChanges, k => k.Name.Contains("Organisation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task QualificationDetails_LoadsFundedOffers()
    {
        // Arrange
        var details = _fixture.Create<GetQualificationDetailsQueryResponse>();
        details.Version = 1;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse> { Success = true, Value = details });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = new GetFeedbackForQualificationFundingByIdQueryResponse { QualificationFundedOffers = new() }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = new GetApplicationsByQanQueryResponse { Applications = new() }
            });

        // Act
        var result = await _controller.QualificationDetails("ABC");

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task QualificationDetails_MapsFundedOffers()
    {
        // Arrange
        var details = _fixture.Create<GetQualificationDetailsQueryResponse>();
        details.Version = 1;

        var funding = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new(),
            Approved = true
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse> { Success = true, Value = details });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse> { Success = true, Value = funding });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = new GetApplicationsByQanQueryResponse { Applications = new() }
            });

        // Act
        var result = await _controller.QualificationDetails("ABC");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);
        Assert.True(model.FundingsOffersOutcomeStatus);
    }

    [Fact]
    public async Task QualificationDetails_LoadsApplications()
    {
        // Arrange
        var details = _fixture.Create<GetQualificationDetailsQueryResponse>();
        details.Version = 1;

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse> { Success = true, Value = details });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = new GetFeedbackForQualificationFundingByIdQueryResponse { QualificationFundedOffers = new() }
            });

        var apps = new GetApplicationsByQanQueryResponse { Applications = new() };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse> { Success = true, Value = apps });

        // Act
        var result = await _controller.QualificationDetails("ABC");

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task QualificationDetails_Exception_RedirectsToError()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _controller.QualificationDetails("ABC");

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetails_Post_LoadsProcessStatusesBeforePermissionCheck()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC" },
            AdditionalActions = new() { ProcessStatusId = Guid.NewGuid(), Note = "" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = model.AdditionalActions.ProcessStatusId.Value, Name = "Decision Required" }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true, Value = new EmptyResponse() });

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsComment_WhenNoProcStatusAndNote()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new() { Note = "Test note", ProcessStatusId = null }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task QualificationDetails_Post_Redirects_WhenNoStatusAndNoNote()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new() { Note = "", ProcessStatusId = null }
        };

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task QualificationDetails_Post_UpdatesStatus_WhenProcessStatusProvided()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new() { ProcessStatusId = Guid.NewGuid(), Note = "" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsCommentAndUpdatesStatus_WhenBothProvided()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new()
            {
                Note = "Some note",
                ProcessStatusId = Guid.NewGuid()
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task QualificationDetails_Post_ReturnsErrorView_WhenMediatorThrows()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new() { Note = "Test", ProcessStatusId = Guid.NewGuid() }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test failure"));

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }



    [Fact]
    public async Task Clear_Empty()
    {
        // Arrange

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Clear(recordsPerPage: 10);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(0, redirect.RouteValues["pageNumber"]);
        Assert.Equal(10, redirect.RouteValues["recordsPerPage"]);
    }

    #region Clear

    [Fact]
    public async Task Clear_RemovesSessionKey()
    {
        // Arrange
        _controller.HttpContext.Session.SetObject("ChangedQualificationFilters", new QualificationFilterSessionModel());

        // Act
        await _controller.Clear();

        // Assert
        Assert.Null(_controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters"));
    }

    [Fact]
    public async Task Clear_InvalidModelState_ReturnsIndexView()
    {
        // Arrange
        _controller.ModelState.AddModelError("x", "y");

        // Act
        var result = await _controller.Clear();

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    //[Fact]
    //public async Task Clear_Exception_ReturnsIndexView()
    //{
    //    Arrange
    //   var throwingSession = new TestSessionThrowing();
    //    _controller.HttpContext.Session = throwingSession;

    //    Act
    //   var result = await _controller.Clear();

    //    Assert
    //   var view = Assert.IsType<ViewResult>(result);
    //    Assert.Equal("Index", view.ViewName);
    //}

    #endregion



    [Fact]
    public async Task Search_WritesFiltersToSession_AndRedirectsToIndex()
    {
        // Arrange
        var viewModel = _fixture.Create<ChangedQualificationsViewModel>();

        // Act
        var result = await _controller.Search(viewModel);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var sessionJson = _controller.HttpContext.Session.GetString("ChangedQualificationFilters");
        Assert.NotNull(sessionJson);

        var sessionModel = System.Text.Json.JsonSerializer.Deserialize<QualificationFilterSessionModel>(sessionJson);

        Assert.Equal(viewModel.Filter.QualificationName, sessionModel!.QualificationName);
        Assert.Equal(viewModel.Filter.Organisation, sessionModel.Organisation);
        Assert.Equal(viewModel.Filter.QAN, sessionModel.QAN);
        Assert.Equal(viewModel.PaginationViewModel.RecordsPerPage, sessionModel.RecordsPerPage);
    }

    #region Search

    [Fact]
    public async Task Search_ResetsPageNumberToOne()
    {
        // Arrange
        var vm = new ChangedQualificationsViewModel
        {
            Filter = new(),
            PaginationViewModel = new() { RecordsPerPage = 20 }
        };

        // Act
        await _controller.Search(vm);

        // Assert
        var session = _controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");
        Assert.Equal(1, session.PageNumber);
    }

    //[Fact]
    //public async Task Search_Exception_ReturnsIndexView()
    //{
    //    // Arrange
    //    var vm = new ChangedQualificationsViewModel { Filter = new(), PaginationViewModel = new() };

    //    var throwingSession = new TestSessionThrowing();
    //    _controller.HttpContext.Session = throwingSession;

    //    // Act
    //    var result = await _controller.Search(vm);

    //    // Assert
    //    var view = Assert.IsType<ViewResult>(result);
    //    Assert.Equal("Index", view.ViewName);
    //}

    [Fact]
    public async Task Search_MapsNullFieldsCorrectly()
    {
        // Arrange
        var vm = new ChangedQualificationsViewModel
        {
            Filter = new(),
            PaginationViewModel = new() { RecordsPerPage = 10 }
        };

        // Act
        await _controller.Search(vm);

        // Assert
        var session = _controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");
        Assert.Equal("", session.QualificationName!);
        Assert.Equal("", session.Organisation);
        Assert.Equal("", session.QAN);
    }

    #endregion


    [Fact]
    public async Task QualificationDetails_Post_Redirects_WhenUserCannotSetStatus()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new()
            {
                ProcessStatusId = Guid.NewGuid(),
                Note = ""
            },
            Version = 1
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Some Other Status" }
                    }
                }
            });

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task QualificationDetails_Post_Redirects_WhenGetProcessStatusesThrows()
    {
        // Arrange
        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new() { Qan = "ABC123" },
            AdditionalActions = new()
            {
                ProcessStatusId = Guid.NewGuid(),
                Note = ""
            },
            Version = 1
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failure"));

        // Act
        var result = await _controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QualificationDetails", redirect.ActionName);
        Assert.Equal("ABC123", redirect.RouteValues["qualificationReference"]);
    }

    [Fact]
    public async Task ExportData_ReturnsFile_WhenExportsExist()
    {
        // Arrange
        var response = new BaseMediatrResponse<GetQualificationsExportResponse>
        {
            Success = true,
            Value = new GetQualificationsExportResponse
            {
                QualificationExports = new List<QualificationExport>
                {
                    new QualificationExport { QANText = "ABC123" }
                }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ExportData();

        // Assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task ExportData_RedirectsToError_WhenResultIsNull()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BaseMediatrResponse<GetQualificationsExportResponse>)null);

        // Act
        var result = await _controller.ExportData();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task ExportData_RedirectsToError_WhenExportsAreNull()
    {
        // Arrange
        var response = new BaseMediatrResponse<GetQualificationsExportResponse>
        {
            Success = true,
            Value = new GetQualificationsExportResponse
            {
                QualificationExports = null
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ExportData();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task ExportData_RedirectsToError_OnException()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));

        // Act
        var result = await _controller.ExportData();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }
}
