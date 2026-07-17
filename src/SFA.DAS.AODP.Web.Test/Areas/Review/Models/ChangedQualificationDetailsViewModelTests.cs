using System.Reflection;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models;

public class ChangedQualificationDetailsViewModelTests
{
    [Fact]
    public void MapToView_WhenEntityIsProvided_ThenMapsAllProperties()
    {
        // Arrange
        var entity = CreateQualificationDetailsQueryResponse();

        var expected = new ChangedQualificationDetailsViewModel
        {
            Id = entity.Id,
            QualificationId = entity.QualificationId,
            VersionFieldChangesId = entity.VersionFieldChangesId,
            ProcessStatusId = entity.ProcessStatusId,
            AdditionalKeyChangesReceivedFlag = entity.AdditionalKeyChangesReceivedFlag,
            LifecycleStageId = entity.LifecycleStageId,
            ChangedFieldNames = entity.VersionFieldChanges,
            OutcomeJustificationNotes = entity.OutcomeJustificationNotes,
            AwardingOrganisationId = entity.AwardingOrganisationId,
            Status = entity.Status,
            Type = entity.Type,
            Ssa = entity.Ssa,
            Name = entity.Name,
            Level = entity.Level,
            SubLevel = entity.SubLevel,
            EqfLevel = entity.EqfLevel,
            GradingType = entity.GradingType,
            GradingScale = entity.GradingScale,
            TotalCredits = entity.TotalCredits,
            Tqt = entity.Tqt,
            Glh = entity.Glh,
            MinimumGlh = entity.MinimumGlh,
            MaximumGlh = entity.MaximumGlh,
            RegulationStartDate = entity.RegulationStartDate,
            OperationalStartDate = entity.OperationalStartDate,
            OperationalEndDate = entity.OperationalEndDate,
            CertificationEndDate = entity.CertificationEndDate,
            ReviewDate = entity.ReviewDate,
            OfferedInEngland = entity.OfferedInEngland,
            OfferedInNi = entity.OfferedInNi,
            OfferedInternationally = entity.OfferedInternationally,
            Specialism = entity.Specialism,
            Pathways = entity.Pathways,
            AssessmentMethods = entity.AssessmentMethods,
            ApprovedForDelFundedProgramme = entity.ApprovedForDelFundedProgramme,
            LinkToSpecification = entity.LinkToSpecification,
            ApprenticeshipStandardReferenceNumber = entity.ApprenticeshipStandardReferenceNumber,
            ApprenticeshipStandardTitle = entity.ApprenticeshipStandardTitle,
            RegulatedByNorthernIreland = entity.RegulatedByNorthernIreland,
            NiDiscountCode = entity.NiDiscountCode,
            GceSizeEquivelence = entity.GceSizeEquivelence,
            GcseSizeEquivelence = entity.GcseSizeEquivelence,
            EntitlementFrameworkDesign = entity.EntitlementFrameworkDesign,
            LastUpdatedDate = entity.LastUpdatedDate,
            UiLastUpdatedDate = entity.UiLastUpdatedDate,
            InsertedDate = entity.InsertedDate,
            InsertedTimestamp = entity.InsertedTimestamp,
            Version = entity.Version,
            AppearsOnPublicRegister = entity.AppearsOnPublicRegister,
            LevelId = entity.LevelId,
            TypeId = entity.TypeId,
            SsaId = entity.SsaId,
            GradingTypeId = entity.GradingTypeId,
            GradingScaleId = entity.GradingScaleId,
            PreSixteen = entity.PreSixteen,
            SixteenToEighteen = entity.SixteenToEighteen,
            EighteenPlus = entity.EighteenPlus,
            NineteenPlus = entity.NineteenPlus,
            ImportStatus = entity.ImportStatus,
            Stage = new ChangedQualificationDetailsViewModel.LifecycleStage
            {
                Id = entity.Stage.Id,
                Name = entity.Stage.Name
            },
            Organisation = new AwardingOrganisation
            {
                Id = entity.Organisation.Id,
                Ukprn = entity.Organisation.Ukprn,
                RecognitionNumber = entity.Organisation.RecognitionNumber,
                NameLegal = entity.Organisation.NameLegal,
                NameOfqual = entity.Organisation.NameOfqual,
                NameGovUk = entity.Organisation.NameGovUk,
                Name_Dsi = entity.Organisation.Name_Dsi,
                Acronym = entity.Organisation.Acronym
            },
            Qual = new ChangedQualificationDetailsViewModel.Qualification
            {
                Id = entity.Qual.Id,
                Qan = entity.Qual.Qan,
                QualificationName = entity.Qual.QualificationName,
                Versions = entity.Qual.Versions.Select(i => new ChangedQualificationDetailsViewModel
                {
                    Id = i.Id,
                    QualificationId = i.QualificationId,
                    VersionFieldChangesId = i.VersionFieldChangesId,
                    ProcessStatusId = i.ProcessStatusId,
                    AdditionalKeyChangesReceivedFlag = i.AdditionalKeyChangesReceivedFlag,
                    LifecycleStageId = i.LifecycleStageId,
                    ChangedFieldNames = i.VersionFieldChanges,
                    OutcomeJustificationNotes = i.OutcomeJustificationNotes,
                    AwardingOrganisationId = i.AwardingOrganisationId,
                    Status = i.Status,
                    Name = i.Name,
                    Type = i.Type,
                    Ssa = i.Ssa,
                    Level = i.Level,
                    SubLevel = i.SubLevel,
                    EqfLevel = i.EqfLevel,
                    GradingType = i.GradingType,
                    GradingScale = i.GradingScale,
                    TotalCredits = i.TotalCredits,
                    Tqt = i.Tqt,
                    Glh = i.Glh,
                    MinimumGlh = i.MinimumGlh,
                    MaximumGlh = i.MaximumGlh,
                    RegulationStartDate = i.RegulationStartDate,
                    OperationalStartDate = i.OperationalStartDate,
                    OperationalEndDate = i.OperationalEndDate,
                    CertificationEndDate = i.CertificationEndDate,
                    ReviewDate = i.ReviewDate,
                    OfferedInEngland = i.OfferedInEngland,
                    OfferedInNi = i.OfferedInNi,
                    OfferedInternationally = i.OfferedInternationally,
                    Specialism = i.Specialism,
                    Pathways = i.Pathways,
                    AssessmentMethods = i.AssessmentMethods,
                    ApprovedForDelFundedProgramme = i.ApprovedForDelFundedProgramme,
                    LinkToSpecification = i.LinkToSpecification,
                    ApprenticeshipStandardReferenceNumber = i.ApprenticeshipStandardReferenceNumber,
                    ApprenticeshipStandardTitle = i.ApprenticeshipStandardTitle,
                    RegulatedByNorthernIreland = i.RegulatedByNorthernIreland,
                    NiDiscountCode = i.NiDiscountCode,
                    GceSizeEquivelence = i.GceSizeEquivelence,
                    GcseSizeEquivelence = i.GcseSizeEquivelence,
                    EntitlementFrameworkDesign = i.EntitlementFrameworkDesign,
                    LastUpdatedDate = i.LastUpdatedDate,
                    UiLastUpdatedDate = i.UiLastUpdatedDate,
                    InsertedDate = i.InsertedDate,
                    InsertedTimestamp = i.InsertedTimestamp,
                    Version = i.Version,
                    AppearsOnPublicRegister = i.AppearsOnPublicRegister,
                    LevelId = i.LevelId,
                    TypeId = i.TypeId,
                    SsaId = i.SsaId,
                    GradingTypeId = i.GradingTypeId,
                    GradingScaleId = i.GradingScaleId,
                    PreSixteen = i.PreSixteen,
                    SixteenToEighteen = i.SixteenToEighteen,
                    EighteenPlus = i.EighteenPlus,
                    NineteenPlus = i.NineteenPlus,
                    ImportStatus = i.ImportStatus,
                    Stage = new ChangedQualificationDetailsViewModel.LifecycleStage
                    {
                        Id = i.Stage.Id,
                        Name = i.Stage.Name
                    },
                    Organisation = new AwardingOrganisation
                    {
                        Id = i.Organisation.Id,
                        Ukprn = i.Organisation.Ukprn,
                        RecognitionNumber = i.Organisation.RecognitionNumber,
                        NameLegal = i.Organisation.NameLegal,
                        NameOfqual = i.Organisation.NameOfqual,
                        NameGovUk = i.Organisation.NameGovUk,
                        Name_Dsi = i.Organisation.Name_Dsi,
                        Acronym = i.Organisation.Acronym
                    },
                    Qual = new ChangedQualificationDetailsViewModel.Qualification
                    {
                        Id = entity.Qual.Id,
                        Qan = entity.Qual.Qan,
                        QualificationName = entity.Qual.QualificationName
                    }
                }).ToList()
            },
            ProcStatus = new ChangedQualificationDetailsViewModel.ProcessStatus
            {
                Id = entity.ProcStatus.Id,
                Name = entity.ProcStatus.Name,
                IsOutcomeDecision = entity.ProcStatus.IsOutcomeDecision
            }
        };

        // Act
        var actual = ChangedQualificationDetailsViewModel.MapToView(entity);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void ChangedFields_WhenChangedFieldNamesIsNull_ThenReturnsEmptyList()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            ChangedFieldNames = null
        };

        // Act
        var actual = viewModel.GetChangedFields();

        // Assert
        actual.ShouldBeEmpty();
    }

    [Fact]
    public void ChangedFields_WhenChangedFieldNamesContainsEmptyEntries_ThenRemovesEmptyEntriesAndTrimsValues()
    {
        // Arrange
        var keyField = KeyField.All[0];

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            ChangedFieldNames = $"  {keyField.Key}  , , NotAKeyField"
        };

        // Act
        var actual = viewModel.GetChangedFields();

        // Assert
        actual.ShouldBeEquivalentTo(new List<string>
        {
            keyField.Key
        });
    }

    [Fact]
    public void ChangedFields_WhenChangedFieldNamesUsesDifferentCasing_ThenMatchesKeyFieldsCaseInsensitively()
    {
        // Arrange
        var keyField = KeyField.All[0];
        var changedFieldName = keyField.Key.ToUpperInvariant();

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            ChangedFieldNames = changedFieldName
        };

        // Act
        var actual = viewModel.GetChangedFields();

        // Assert
        actual.ShouldBeEquivalentTo(new List<string>
        {
            changedFieldName
        });
    }

    [Fact]
    public void ChangeFieldsForDisplay_WhenChangedFieldsExist_ThenReturnsCommaSeparatedChangedFields()
    {
        // Arrange
        var firstKeyField = KeyField.All[0];
        var secondKeyField = KeyField.All[1];

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            ChangedFieldNames = $"{firstKeyField.Key}, {secondKeyField.Key}"
        };

        // Act
        var actual = viewModel.ChangeFieldsForDisplay;

        // Assert
        actual.ShouldBe($"{firstKeyField.Key}, {secondKeyField.Key}");
    }

    [Fact]
    public void Priority_WhenStatusIsNoActionRequired_ThenReturnsGreen()
    {
        // Arrange
        var redKeyField = KeyField.All.First(x => x.Priority == KeyFieldPriority.Red);

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Status = ActionTypeEnum.NoActionRequired,
            ChangedFieldNames = redKeyField.Key
        };

        // Act
        var actual = viewModel.Priority;

        // Assert
        actual.ShouldBe(KeyFieldPriority.Green);
    }

    [Fact]
    public void Priority_WhenThereAreNoChangedFields_ThenReturnsGreen()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Status = "Some status",
            ChangedFieldNames = null
        };

        // Act
        var actual = viewModel.Priority;

        // Assert
        actual.ShouldBe(KeyFieldPriority.Green);
    }

    [Fact]
    public void Priority_WhenChangedFieldsContainRedKeyField_ThenReturnsRed()
    {
        // Arrange
        var redKeyField = KeyField.All.First(x => x.Priority == KeyFieldPriority.Red);

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Status = "Some status",
            ChangedFieldNames = redKeyField.Key
        };

        // Act
        var actual = viewModel.Priority;

        // Assert
        actual.ShouldBe(KeyFieldPriority.Red);
    }

    [Fact]
    public void Priority_WhenChangedFieldsContainYellowKeyFieldAndNoRedKeyField_ThenReturnsYellow()
    {
        // Arrange
        var yellowKeyField = KeyField.All.First(x => x.Priority == KeyFieldPriority.Yellow);

        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Status = "Some status",
            ChangedFieldNames = yellowKeyField.Key
        };

        // Act
        var actual = viewModel.Priority;

        // Assert
        actual.ShouldBe(KeyFieldPriority.Yellow);
    }

    [Fact]
    public void IsQualificationCompleted_WhenStageNameIsCompleted_ThenReturnsTrue()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Stage = new ChangedQualificationDetailsViewModel.LifecycleStage
            {
                Name = "Completed"
            }
        };

        // Act
        var actual = viewModel.IsQualificationCompleted;

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void IsQualificationCompleted_WhenStageNameIsCompletedWithDifferentCasing_ThenReturnsTrue()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Stage = new ChangedQualificationDetailsViewModel.LifecycleStage
            {
                Name = "completed"
            }
        };

        // Act
        var actual = viewModel.IsQualificationCompleted;

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void IsQualificationCompleted_WhenStageNameIsNotCompleted_ThenReturnsFalse()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Stage = new ChangedQualificationDetailsViewModel.LifecycleStage
            {
                Name = "In progress"
            }
        };

        // Act
        var actual = viewModel.IsQualificationCompleted;

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public void IsQualificationCompleted_WhenStageIsNull_ThenReturnsFalse()
    {
        // Arrange
        var viewModel = new ChangedQualificationDetailsViewModel
        {
            Stage = null!
        };

        // Act
        var actual = viewModel.IsQualificationCompleted;

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public void ProcessStatusImplicitOperator_WhenModelIsProvided_ThenMapsAllProperties()
    {
        // Arrange
        var model = new GetProcessStatusesQueryResponse.ProcessStatus
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Approved",
            IsOutcomeDecision = 1
        };

        var expected = new ChangedQualificationDetailsViewModel.ProcessStatus
        {
            Id = model.Id,
            Name = model.Name,
            IsOutcomeDecision = model.IsOutcomeDecision
        };

        // Act
        ChangedQualificationDetailsViewModel.ProcessStatus actual = model;

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void MapFundedOffers_WhenFeedbackContainsFundedOffers_ThenAddsFundingDetails()
    {
        // Arrange
        var feedback = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers =
            [
                new()
                {
                    FundedOfferName = "Adult Skills",
                    StartDate = new DateOnly(2026, 8, 1),
                    EndDate = new DateOnly(2027, 7, 31)
                },

                new()
                {
                    FundedOfferName = "Advanced Learner Loans",
                    StartDate = new DateOnly(2026, 9, 1),
                    EndDate = null
                }
            ]
        };

        var viewModel = new ChangedQualificationDetailsViewModel();

        var expected = new List<ChangedQualificationDetailsViewModel.OfferFundingDetails>
        {
            new()
            {
                FundingOfferName = "Adult Skills",
                StartDate = new DateOnly(2026, 8, 1),
                EndDate = new DateOnly(2027, 7, 31)
            },
            new()
            {
                FundingOfferName = "Advanced Learner Loans",
                StartDate = new DateOnly(2026, 9, 1),
                EndDate = null
            }
        };

        // Act
        InvokeMapFundedOffers(viewModel, feedback);

        // Assert
        viewModel.FundingDetails.ShouldBeEquivalentTo(expected);
    }

    private static void InvokeMapFundedOffers(
        ChangedQualificationDetailsViewModel viewModel,
        GetFeedbackForQualificationFundingByIdQueryResponse feedback)
    {
        var method = typeof(ChangedQualificationDetailsViewModel)
            .GetMethod("MapFundedOffers", BindingFlags.Instance | BindingFlags.NonPublic);

        method.ShouldNotBeNull();

        method.Invoke(viewModel, [feedback]);
    }

    private static GetQualificationDetailsQueryResponse CreateQualificationDetailsQueryResponse()
    {
        var rootId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var qualificationId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var versionFieldChangesId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var processStatusId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        var lifecycleStageId = Guid.Parse("00000000-0000-0000-0000-000000000005");
        var awardingOrganisationId = Guid.Parse("00000000-0000-0000-0000-000000000006");

        return new GetQualificationDetailsQueryResponse
        {
            Id = rootId,
            QualificationId = qualificationId,
            VersionFieldChangesId = versionFieldChangesId,
            ProcessStatusId = processStatusId,
            AdditionalKeyChangesReceivedFlag = 1,
            LifecycleStageId = lifecycleStageId,
            VersionFieldChanges = "Name, Type",
            OutcomeJustificationNotes = "Outcome justification notes",
            AwardingOrganisationId = awardingOrganisationId,
            Status = "Status",
            Type = "Type",
            Ssa = "SSA",
            Name = "Qualification name",
            Level = "Level 3",
            SubLevel = "Sub level",
            EqfLevel = "EQF 3",
            GradingType = "Pass merit distinction",
            GradingScale = "A*-E",
            TotalCredits = 120,
            Tqt = 500,
            Glh = 400,
            MinimumGlh = 300,
            MaximumGlh = 450,
            RegulationStartDate = new DateTime(2026, 1, 1, 9, 0, 0),
            OperationalStartDate = new DateTime(2026, 2, 1, 9, 0, 0),
            OperationalEndDate = new DateTime(2027, 2, 1, 9, 0, 0),
            CertificationEndDate = new DateTime(2028, 2, 1, 9, 0, 0),
            ReviewDate = new DateTime(2029, 2, 1, 9, 0, 0),
            OfferedInEngland = true,
            OfferedInNi = false,
            OfferedInternationally = true,
            Specialism = "Specialism",
            Pathways = "Pathways",
            AssessmentMethods = "Assessment methods",
            ApprovedForDelFundedProgramme = "Yes",
            LinkToSpecification = "https://example.com/specification",
            ApprenticeshipStandardReferenceNumber = "ST0001",
            ApprenticeshipStandardTitle = "Software developer",
            RegulatedByNorthernIreland = true,
            NiDiscountCode = "NI123",
            GceSizeEquivelence = "1 GCE",
            GcseSizeEquivelence = "4 GCSE",
            EntitlementFrameworkDesign = "Framework design",
            LastUpdatedDate = new DateTime(2026, 3, 1, 9, 0, 0),
            UiLastUpdatedDate = new DateTime(2026, 3, 2, 9, 0, 0),
            InsertedDate = new DateTime(2026, 3, 3, 9, 0, 0),
            InsertedTimestamp = new DateTime(2026, 3, 4, 9, 0, 0),
            Version = 2,
            AppearsOnPublicRegister = true,
            LevelId = 3,
            TypeId = 4,
            SsaId = 5,
            GradingTypeId = 6,
            GradingScaleId = 7,
            PreSixteen = false,
            SixteenToEighteen = true,
            EighteenPlus = true,
            NineteenPlus = false,
            ImportStatus = "Imported",
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = lifecycleStageId,
                Name = "Completed"
            },
            Organisation = new AwardingOrganisation
            {
                Id = awardingOrganisationId,
                Ukprn = 12345678,
                RecognitionNumber = "RN123",
                NameLegal = "Legal name",
                NameOfqual = "Ofqual name",
                NameGovUk = "Gov UK name",
                Name_Dsi = "DSI name",
                Acronym = "AO"
            },
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = qualificationId,
                Qan = "12345678",
                QualificationName = "Root qualification name",
                Versions =
                [
                    CreateVersionQualificationDetailsQueryResponse(qualificationId, lifecycleStageId,
                        awardingOrganisationId)
                ]
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = processStatusId,
                Name = "Approved",
                IsOutcomeDecision = 1
            }
        };
    }

    private static GetQualificationDetailsQueryResponse CreateVersionQualificationDetailsQueryResponse(
        Guid qualificationId,
        Guid lifecycleStageId,
        Guid awardingOrganisationId)
    {
        return new GetQualificationDetailsQueryResponse
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            QualificationId = qualificationId,
            VersionFieldChangesId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            ProcessStatusId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            AdditionalKeyChangesReceivedFlag = 0,
            LifecycleStageId = lifecycleStageId,
            VersionFieldChanges = "Level",
            OutcomeJustificationNotes = "Version notes",
            AwardingOrganisationId = awardingOrganisationId,
            Status = "Version status",
            Type = "Version type",
            Ssa = "Version SSA",
            Name = "Version qualification name",
            Level = "Level 4",
            SubLevel = "Version sub level",
            EqfLevel = "EQF 4",
            GradingType = "Version grading type",
            GradingScale = "Version grading scale",
            TotalCredits = 60,
            Tqt = 250,
            Glh = 200,
            MinimumGlh = 150,
            MaximumGlh = 225,
            RegulationStartDate = new DateTime(2025, 1, 1, 9, 0, 0),
            OperationalStartDate = new DateTime(2025, 2, 1, 9, 0, 0),
            OperationalEndDate = new DateTime(2026, 2, 1, 9, 0, 0),
            CertificationEndDate = new DateTime(2027, 2, 1, 9, 0, 0),
            ReviewDate = new DateTime(2028, 2, 1, 9, 0, 0),
            OfferedInEngland = false,
            OfferedInNi = true,
            OfferedInternationally = false,
            Specialism = "Version specialism",
            Pathways = "Version pathways",
            AssessmentMethods = "Version assessment methods",
            ApprovedForDelFundedProgramme = "No",
            LinkToSpecification = "https://example.com/version-specification",
            ApprenticeshipStandardReferenceNumber = "ST0002",
            ApprenticeshipStandardTitle = "Data analyst",
            RegulatedByNorthernIreland = false,
            NiDiscountCode = "NI456",
            GceSizeEquivelence = "2 GCE",
            GcseSizeEquivelence = "5 GCSE",
            EntitlementFrameworkDesign = "Version framework design",
            LastUpdatedDate = new DateTime(2025, 3, 1, 9, 0, 0),
            UiLastUpdatedDate = new DateTime(2025, 3, 2, 9, 0, 0),
            InsertedDate = new DateTime(2025, 3, 3, 9, 0, 0),
            InsertedTimestamp = new DateTime(2025, 3, 4, 9, 0, 0),
            Version = 1,
            AppearsOnPublicRegister = false,
            LevelId = 8,
            TypeId = 9,
            SsaId = 10,
            GradingTypeId = 11,
            GradingScaleId = 12,
            PreSixteen = true,
            SixteenToEighteen = false,
            EighteenPlus = false,
            NineteenPlus = true,
            ImportStatus = "Version imported",
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = lifecycleStageId,
                Name = "In progress"
            },
            Organisation = new AwardingOrganisation
            {
                Id = awardingOrganisationId,
                Ukprn = 87654321,
                RecognitionNumber = "RN456",
                NameLegal = "Version legal name",
                NameOfqual = "Version Ofqual name",
                NameGovUk = "Version Gov UK name",
                Name_Dsi = "Version DSI name",
                Acronym = "VAO"
            }
        };
    }
}