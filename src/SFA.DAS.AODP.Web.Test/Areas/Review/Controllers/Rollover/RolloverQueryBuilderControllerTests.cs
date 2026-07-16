using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using Shouldly;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers.Rollover;

public class RolloverQueryBuilderControllerTests : RolloverControllerTestBase
{
    private static readonly QualificationLevel Level = QualificationLevel.Level3;
    private static readonly QualificationType Type = QualificationType.GCEAlevel;
    private static readonly SectorSubjectArea SectorSubjectArea = SectorSubjectArea.Engineering;
    private static readonly AwardingOrganisation AwardingOrganisation = new()
    {
        Id = Guid.NewGuid(),
        RecognitionNumber = "RN1",
        NameOfqual = "Awarding organisation"
    };

    [Fact]
    public async Task SelectLevelsGet_WhenListGenerationWasNotSelected_ShouldRedirectToSelectCandidates()
    {
        // Arrange
        var controller = CreateController(CreateSession(new AODP.Domain.Rollover.Rollover()));

        // Act
        var result = await controller.SelectLevels();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectCandidates));
        MediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelectLevelsGet_ShouldRestoreSelectionAndDisplayAvailableLevels()
    {
        // Arrange
        var rollover = new AODP.Domain.Rollover.Rollover
        {
            SelectCandidates = new RolloverSelectCandidates
            {
                SelectedOption = SelectCandidatesForRollover.GenerateAList
            },
            QueryBuilderFilters = new QueryBuilderFilters().SetLevels([Level])
        };
        SetupLevels(Level, QualificationLevel.Level4);
        var controller = CreateController(CreateSession(rollover));

        // Act
        var result = await controller.SelectLevels();

        // Assert
        var model = result.ShouldBeViewWithModel<SelectQualificationLevelsViewModel>();
        model.SelectedLevels.ShouldBe([Level]);
        model.Levels.Single(item => item.Value == Level.Id.ToString()).IsChecked.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectLevelsPost_WhenSelectionIsInvalid_ShouldRedisplayAvailableLevels()
    {
        // Arrange
        SetupLevels(Level);
        var controller = CreateController(CreateSession());
        controller.ModelState.AddModelError(nameof(SelectQualificationLevelsViewModel.SelectedLevels), "Required");
        var model = new SelectQualificationLevelsViewModel();

        // Act
        var result = await controller.SelectLevels(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectQualificationLevelsViewModel>().Levels.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task SelectLevelsPost_WhenSelectAllIsRequested_ShouldCheckEveryAvailableLevel()
    {
        // Arrange
        SetupLevels(Level, QualificationLevel.Level4);
        var controller = CreateController(CreateSession());
        controller.ModelState.AddModelError(nameof(SelectQualificationLevelsViewModel.SelectedLevels), "Required");
        var model = new SelectQualificationLevelsViewModel();

        // Act
        var result = await controller.SelectLevels(model, "selectAll");

        // Assert
        var returnedModel = result.ShouldBeViewWithModel<SelectQualificationLevelsViewModel>();
        returnedModel.Levels.ShouldAllBe(item => item.IsChecked);
        controller.ModelState.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectLevelsPost_WhenSelectionIsValid_ShouldSaveLevelsAndContinue()
    {
        // Arrange
        SetupLevels(Level);
        var session = CreateSession();
        var controller = CreateController(session);
        var model = new SelectQualificationLevelsViewModel { SelectedLevels = [Level] };

        // Act
        var result = await controller.SelectLevels(model, "continue");

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectTypes));
        ReadSession(session).QueryBuilderFilters.Levels.ShouldBe([Level]);
    }

    [Fact]
    public async Task SelectTypesGet_WhenLevelsAreMissing_ShouldRedirectToSelectLevels()
    {
        // Arrange
        var controller = CreateController(CreateSession());

        // Act
        var result = await controller.SelectTypes();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectLevels));
        MediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelectTypesPost_WhenSelectAllIsRequested_ShouldCheckEveryAvailableType()
    {
        // Arrange
        SetupTypes(Type, QualificationType.Project);
        var controller = CreateController(CreateSession(FiltersWithLevels()));
        controller.ModelState.AddModelError(nameof(SelectQualificationTypesViewModel.SelectedTypes), "Required");
        var model = new SelectQualificationTypesViewModel();

        // Act
        var result = await controller.SelectTypes(model, "selectAll");

        // Assert
        var returnedModel = result.ShouldBeViewWithModel<SelectQualificationTypesViewModel>();
        returnedModel.Types.ShouldAllBe(item => item.IsChecked);
        controller.ModelState.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectTypesPost_WhenSelectionIsValid_ShouldSaveTypesAndContinue()
    {
        // Arrange
        SetupTypes(Type);
        var session = CreateSession(FiltersWithLevels());
        var controller = CreateController(session);
        var model = new SelectQualificationTypesViewModel { SelectedTypes = [Type] };

        // Act
        var result = await controller.SelectTypes(model, "continue");

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectSectorSubjectArea));
        ReadSession(session).QueryBuilderFilters.Types.ShouldBe([Type]);
    }

    [Fact]
    public async Task SelectSectorSubjectAreaGet_WhenTypesAreMissing_ShouldRedirectToSelectTypes()
    {
        // Arrange
        var controller = CreateController(CreateSession(FiltersWithLevels()));

        // Act
        var result = await controller.SelectSectorSubjectArea();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectTypes));
        MediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelectSectorSubjectAreaPost_WhenSelectionTypeIsMissing_ShouldAddBusinessValidationError()
    {
        // Arrange
        SetupSectorSubjectAreas(SectorSubjectArea);
        var controller = CreateController(CreateSession(FiltersWithTypes()));
        var model = new SelectSectorSubjectAreasModel();

        // Act
        var result = await controller.SelectSectorSubjectArea(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectSectorSubjectAreasModel>();
        controller.ModelState[nameof(model.SelectionType)]!.Errors.Single().ErrorMessage
            .ShouldBe("Select if you want to rollover all SSAs or only a selection");
    }

    [Fact]
    public async Task SelectSectorSubjectAreaPost_WhenSpecificSelectionIsEmpty_ShouldAddBusinessValidationError()
    {
        // Arrange
        SetupSectorSubjectAreas(SectorSubjectArea);
        var controller = CreateController(CreateSession(FiltersWithTypes()));
        var model = new SelectSectorSubjectAreasModel
        {
            SelectionType = SectorSubjectAreaSelectionType.SpecificSelection
        };

        // Act
        var result = await controller.SelectSectorSubjectArea(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectSectorSubjectAreasModel>();
        controller.ModelState[nameof(model.SelectedSectorSubjectAreas)]!.Errors.Single().ErrorMessage
            .ShouldBe("You must select at least one SSA");
    }

    [Fact]
    public async Task SelectSectorSubjectAreaPost_WhenAllIsSelected_ShouldSaveEveryAvailableAreaAndContinue()
    {
        // Arrange
        SetupSectorSubjectAreas(SectorSubjectArea, SectorSubjectArea.Science);
        var session = CreateSession(FiltersWithTypes());
        var controller = CreateController(session);
        var model = new SelectSectorSubjectAreasModel
        {
            SelectionType = SectorSubjectAreaSelectionType.All
        };

        // Act
        var result = await controller.SelectSectorSubjectArea(model, "continue");

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectAwardingOrganisations));
        var savedFilters = ReadSession(session).QueryBuilderFilters;
        savedFilters.SectorSubjectAreas.ShouldBe([SectorSubjectArea, SectorSubjectArea.Science]);
        savedFilters.SectorSubjectAreasSelectionType.ShouldBe(SectorSubjectAreaSelectionType.All);
    }

    [Fact]
    public async Task SelectAwardingOrganisationsGet_WhenSectorSubjectAreasAreMissing_ShouldRedirectBack()
    {
        // Arrange
        var controller = CreateController(CreateSession(FiltersWithTypes()));

        // Act
        var result = await controller.SelectAwardingOrganisations();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectSectorSubjectArea));
        MediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SelectAwardingOrganisationsPost_WhenSelectionTypeIsMissing_ShouldAddBusinessValidationError()
    {
        // Arrange
        SetupAwardingOrganisations(AwardingOrganisation);
        var controller = CreateController(CreateSession(ValidFilters()));
        var model = new SelectAwardingOrganisationsViewModel();

        // Act
        var result = await controller.SelectAwardingOrganisations(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectAwardingOrganisationsViewModel>();
        controller.ModelState[nameof(model.SelectionType)]!.Errors.Single().ErrorMessage
            .ShouldBe("Select if you want to rollover all awarding organisations or only a selection");
    }

    [Fact]
    public async Task SelectAwardingOrganisationsPost_WhenSpecificSelectionIsEmpty_ShouldAddBusinessValidationError()
    {
        // Arrange
        SetupAwardingOrganisations(AwardingOrganisation);
        var controller = CreateController(CreateSession(ValidFilters()));
        var model = new SelectAwardingOrganisationsViewModel
        {
            SelectionType = AwardingOrganisationSelectionType.SpecificSelection
        };

        // Act
        var result = await controller.SelectAwardingOrganisations(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectAwardingOrganisationsViewModel>();
        controller.ModelState[nameof(model.SelectedAwardingOrganisations)]!.Errors.Single().ErrorMessage
            .ShouldBe("You must select at least one awarding organisation");
    }

    [Fact]
    public async Task SelectAwardingOrganisationsPost_WhenAllIsSelected_ShouldSaveEveryOrganisationAndContinue()
    {
        // Arrange
        SetupAwardingOrganisations(AwardingOrganisation);
        var session = CreateSession(ValidFilters());
        var controller = CreateController(session);
        var model = new SelectAwardingOrganisationsViewModel
        {
            SelectionType = AwardingOrganisationSelectionType.All
        };

        // Act
        var result = await controller.SelectAwardingOrganisations(model, "continue");

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.CheckYourAnswers));
        var savedFilters = ReadSession(session).QueryBuilderFilters;
        savedFilters.SelectedAwardingOrganisationIds.ShouldBe([AwardingOrganisation.RecognitionNumber!]);
        savedFilters.AwardingOrganisationSelectionType.ShouldBe(AwardingOrganisationSelectionType.All);
    }

    [Fact]
    public async Task CheckYourAnswersGet_WhenLevelsAreMissing_ShouldRedirectToSelectLevels()
    {
        // Arrange
        var controller = CreateController(CreateSession());

        // Act
        var result = await controller.CheckYourAnswers();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectLevels));
        MediatorMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CheckYourAnswersGet_WhenFiltersAreValid_ShouldBuildSummaryAndCandidateCount()
    {
        // Arrange
        var filters = ValidFilters().SetAwardingOrganisations(
            [AwardingOrganisation.RecognitionNumber!], [AwardingOrganisation],
            AwardingOrganisationSelectionType.SpecificSelection);
        SetupQualificationVersions(new RolloverQueryBuilderCandidatesDto { Id = Guid.NewGuid() });
        var controller = CreateController(CreateSession(filters));

        // Act
        var result = await controller.CheckYourAnswers();

        // Assert
        var model = result.ShouldBeViewWithModel<CheckYourAnswersViewModel>();
        model.Levels.ShouldBe([Level]);
        model.Types.ShouldBe([Type]);
        model.SectorSubjectAreas.ShouldBe([SectorSubjectArea]);
        model.AwardingOrganisations.ShouldBe([AwardingOrganisation]);
        model.CandidateCount.ShouldBe(1);
    }

    [Fact]
    public async Task CheckYourAnswersPost_WhenNoCandidatesMatch_ShouldReturnToSelectLevels()
    {
        // Arrange
        SetupQualificationVersions();
        var controller = CreateController(CreateSession(ValidFilters()));

        // Act
        var result = await controller.CheckYourAnswers(new CheckYourAnswersViewModel());

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectLevels));
    }

    [Fact]
    public async Task CheckYourAnswersPost_WhenCandidatesMatch_ShouldMapAndSaveCandidatesThenContinue()
    {
        // Arrange
        var candidate = new RolloverQueryBuilderCandidatesDto
        {
            Id = Guid.NewGuid(),
            QualificationVersionId = Guid.NewGuid(),
            QualificationNumber = "123/4567/8",
            QualificationName = "Qualification",
            AcademicYear = "2026",
            FundingOfferId = Guid.NewGuid(),
            FundingOfferName = "Offer",
            PreviousFundingEndDate = new DateTime(2026, 7, 31)
        };
        SetupQualificationVersions(candidate);
        var session = CreateSession(ValidFilters());
        var controller = CreateController(session);

        // Act
        var result = await controller.CheckYourAnswers(new CheckYourAnswersViewModel());

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.FundingStreamInclusionExclusion));
        var savedCandidate = ReadSession(session).RolloverCandidates.ShouldHaveSingleItem();
        savedCandidate.RolloverCandidateId.ShouldBe(candidate.Id);
        savedCandidate.QualificationVersionId.ShouldBe(candidate.QualificationVersionId);
        savedCandidate.QualificationNumber.ShouldBe(candidate.QualificationNumber);
        savedCandidate.FundingOfferId.ShouldBe(candidate.FundingOfferId);
        savedCandidate.FundingApprovalEndDate.ShouldBe(candidate.PreviousFundingEndDate);
    }

    [Fact]
    public async Task SelectTypesGet_ShouldRestoreSelectionAndDisplayAvailableTypes()
    {
        // Arrange
        var filters = FiltersWithLevels().SetTypes([Type]);
        SetupTypes(Type, QualificationType.Project);
        var controller = CreateController(CreateSession(filters));

        // Act
        var result = await controller.SelectTypes();

        // Assert
        var model = result.ShouldBeViewWithModel<SelectQualificationTypesViewModel>();
        model.SelectedTypes.ShouldBe([Type]);
        model.Types.Single(item => item.Value == Type.Id.ToString()).IsChecked.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectTypesPost_WhenSelectionIsInvalid_ShouldRedisplayAvailableTypes()
    {
        // Arrange
        SetupTypes(Type);
        var controller = CreateController(CreateSession(FiltersWithLevels()));
        controller.ModelState.AddModelError(nameof(SelectQualificationTypesViewModel.SelectedTypes), "Required");
        var model = new SelectQualificationTypesViewModel();

        // Act
        var result = await controller.SelectTypes(model, "continue");

        // Assert
        result.ShouldBeViewWithModel<SelectQualificationTypesViewModel>().Types.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task SelectSectorSubjectAreaGet_ShouldRestoreSelectionTypeAndSelectedAreas()
    {
        // Arrange
        var filters = FiltersWithTypes().SetSectorSubjectAreas(
            [SectorSubjectArea], [SectorSubjectArea, SectorSubjectArea.Science],
            SectorSubjectAreaSelectionType.SpecificSelection);
        SetupSectorSubjectAreas(SectorSubjectArea, SectorSubjectArea.Science);
        var controller = CreateController(CreateSession(filters));

        // Act
        var result = await controller.SelectSectorSubjectArea();

        // Assert
        var model = result.ShouldBeViewWithModel<SelectSectorSubjectAreasModel>();
        model.SelectionType.ShouldBe(SectorSubjectAreaSelectionType.SpecificSelection);
        model.SelectedSectorSubjectAreas.ShouldBe([SectorSubjectArea]);
        model.SectorSubjectAreas.Single(item => item.Value == SectorSubjectArea.Code).IsChecked.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectSectorSubjectAreaPost_WhenSelectAllIsRequestedForSpecificSelection_ShouldCheckEveryArea()
    {
        // Arrange
        SetupSectorSubjectAreas(SectorSubjectArea, SectorSubjectArea.Science);
        var controller = CreateController(CreateSession(FiltersWithTypes()));
        var model = new SelectSectorSubjectAreasModel
        {
            SelectionType = SectorSubjectAreaSelectionType.SpecificSelection
        };

        // Act
        var result = await controller.SelectSectorSubjectArea(model, "selectAll");

        // Assert
        var returnedModel = result.ShouldBeViewWithModel<SelectSectorSubjectAreasModel>();
        returnedModel.SectorSubjectAreas.ShouldAllBe(item => item.IsChecked);
        returnedModel.SelectedSectorSubjectAreas.ShouldBe([SectorSubjectArea, SectorSubjectArea.Science]);
    }

    [Fact]
    public async Task SelectAwardingOrganisationsGet_ShouldRestoreSelectionAndDisplayAvailableOrganisations()
    {
        // Arrange
        var filters = ValidFilters().SetAwardingOrganisations(
            [AwardingOrganisation.RecognitionNumber!], [AwardingOrganisation],
            AwardingOrganisationSelectionType.SpecificSelection);
        SetupAwardingOrganisations(AwardingOrganisation);
        var controller = CreateController(CreateSession(filters));

        // Act
        var result = await controller.SelectAwardingOrganisations();

        // Assert
        var model = result.ShouldBeViewWithModel<SelectAwardingOrganisationsViewModel>();
        model.SelectionType.ShouldBe(AwardingOrganisationSelectionType.SpecificSelection);
        model.SelectedAwardingOrganisations.ShouldBe([AwardingOrganisation.RecognitionNumber!]);
        model.AwardingOrganisations.ShouldHaveSingleItem().IsChecked.ShouldBeTrue();
    }

    [Fact]
    public async Task SelectAwardingOrganisationsPost_WhenSelectAllIsRequestedForSpecificSelection_ShouldCheckEveryOrganisation()
    {
        // Arrange
        SetupAwardingOrganisations(AwardingOrganisation);
        var controller = CreateController(CreateSession(ValidFilters()));
        var model = new SelectAwardingOrganisationsViewModel
        {
            SelectionType = AwardingOrganisationSelectionType.SpecificSelection
        };

        // Act
        var result = await controller.SelectAwardingOrganisations(model, "selectAll");

        // Assert
        var returnedModel = result.ShouldBeViewWithModel<SelectAwardingOrganisationsViewModel>();
        returnedModel.AwardingOrganisations.ShouldHaveSingleItem().IsChecked.ShouldBeTrue();
        returnedModel.SelectedAwardingOrganisations.ShouldBe([AwardingOrganisation.RecognitionNumber!]);
    }

    [Fact]
    public async Task CheckYourAnswersGet_WhenTypesAreMissing_ShouldRedirectToSelectTypes()
    {
        // Arrange
        var controller = CreateController(CreateSession(FiltersWithLevels()));

        // Act
        var result = await controller.CheckYourAnswers();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectTypes));
    }

    [Fact]
    public async Task CheckYourAnswersGet_WhenSectorSubjectAreasAreMissing_ShouldRedirectToSectorSubjectArea()
    {
        // Arrange
        var controller = CreateController(CreateSession(FiltersWithTypes()));

        // Act
        var result = await controller.CheckYourAnswers();

        // Assert
        result.ShouldBeRedirectTo(nameof(RolloverController.SelectSectorSubjectArea));
    }

    private void SetupLevels(params QualificationLevel[] levels)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetLevelsForRolloverQueryBuilderQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetLevelsForRolloverQueryBuilderQueryResponse { Levels = levels }));
    }

    private void SetupTypes(params QualificationType[] types)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetTypesForRolloverQueryBuilderQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetTypesForRolloverQueryBuilderQueryResponse { Types = types }));
    }

    private void SetupSectorSubjectAreas(params SectorSubjectArea[] sectorSubjectAreas)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetSectorSubjectAreaForRolloverQueryBuilderQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse
            {
                SectorSubjectAreas = sectorSubjectAreas
            }));
    }

    private void SetupAwardingOrganisations(params AwardingOrganisation[] awardingOrganisations)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetAwardingOrganisationsForRolloverQueryBuilderQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse
            {
                AwardingOrganisations = awardingOrganisations
            }));
    }

    private void SetupQualificationVersions(params RolloverQueryBuilderCandidatesDto[] qualificationVersions)
    {
        MediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetQualificationVersionsForRolloverQueryBuilderQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetQualificationVersionsForRolloverQueryBuilderQueryResponse
            {
                QualificationVersions = qualificationVersions
            }));
    }

    private static BaseMediatrResponse<T> Success<T>(T value) where T : class, new() => new()
    {
        Success = true,
        Value = value
    };

    private static QueryBuilderFilters FiltersWithLevels() =>
        new QueryBuilderFilters().SetLevels([Level]);

    private static QueryBuilderFilters FiltersWithTypes() =>
        FiltersWithLevels().SetTypes([Type]);

    private static QueryBuilderFilters ValidFilters() =>
        FiltersWithTypes().SetSectorSubjectAreas(
            [SectorSubjectArea], [SectorSubjectArea], SectorSubjectAreaSelectionType.SpecificSelection);

    private static ISession CreateSession(QueryBuilderFilters? filters = null) =>
        CreateSession(new AODP.Domain.Rollover.Rollover
        {
            QueryBuilderFilters = filters ?? new QueryBuilderFilters()
        });

    private static ISession CreateSession(AODP.Domain.Rollover.Rollover rollover)
    {
        var session = new TestSession();
        session.SetString("RolloverSession", System.Text.Json.JsonSerializer.Serialize(rollover));
        return session;
    }

    private static AODP.Domain.Rollover.Rollover ReadSession(ISession session) =>
        System.Text.Json.JsonSerializer.Deserialize<AODP.Domain.Rollover.Rollover>(
            session.GetString("RolloverSession")!)!;
}

internal static class RolloverQueryBuilderControllerTestAssertions
{
    public static void ShouldBeRedirectTo(this IActionResult result, string actionName)
    {
        var redirect = result.ShouldBeOfType<RedirectToActionResult>();
        redirect.ActionName.ShouldBe(actionName);
    }

    public static TModel ShouldBeViewWithModel<TModel>(this IActionResult result)
    {
        var view = result.ShouldBeOfType<ViewResult>();
        return view.Model.ShouldBeOfType<TModel>();
    }
}
