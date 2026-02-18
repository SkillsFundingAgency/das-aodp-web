using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models;

public class NewQualificationDetailsViewModelTests
{
    [Fact]
    public void ImplicitOperator_MapsGetQualificationDetailsQueryResponse_ToViewModel()
    {
        // Arrange
        var response = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            VersionFieldChangesId = Guid.NewGuid(),
            ProcessStatusId = Guid.NewGuid(),
            AdditionalKeyChangesReceivedFlag = 2,
            LifecycleStageId = Guid.NewGuid(),
            OutcomeJustificationNotes = "Some notes",
            AwardingOrganisationId = Guid.NewGuid(),
            Status = "StatusValue",
            Type = "TypeValue",
            Ssa = "SSA",
            Level = "LevelOne",
            SubLevel = "Sub",
            EqfLevel = "EQF1",
            GradingType = "TypeA",
            GradingScale = "ScaleA",
            TotalCredits = 10,
            Tqt = 20,
            Glh = 30,
            MinimumGlh = 5,
            MaximumGlh = 40,
            RegulationStartDate = new DateTime(2020, 1, 1),
            OperationalStartDate = new DateTime(2020, 2, 1),
            OperationalEndDate = new DateTime(2025, 2, 1),
            CertificationEndDate = new DateTime(2026, 3, 1),
            ReviewDate = new DateTime(2024, 4, 1),
            OfferedInEngland = true,
            OfferedInNi = false,
            OfferedInternationally = true,
            Specialism = "Spec",
            Pathways = "Path",
            AssessmentMethods = "AMethod",
            ApprovedForDelFundedProgramme = "Yes",
            LinkToSpecification = "http://spec",
            ApprenticeshipStandardReferenceNumber = "ASRN",
            ApprenticeshipStandardTitle = "ASTitle",
            RegulatedByNorthernIreland = false,
            NiDiscountCode = "NID",
            GceSizeEquivelence = "GCE",
            GcseSizeEquivelence = "GCSE",
            EntitlementFrameworkDesign = "EFD",
            LastUpdatedDate = new DateTime(2021, 5, 1),
            UiLastUpdatedDate = new DateTime(2021, 6, 1),
            InsertedDate = new DateTime(2019, 7, 1),
            InsertedTimestamp = DateTime.UtcNow,
            Version = 3,
            AppearsOnPublicRegister = true,
            LevelId = 1,
            TypeId = 2,
            SsaId = 3,
            GradingTypeId = 4,
            GradingScaleId = 5,
            PreSixteen = true,
            SixteenToEighteen = false,
            EighteenPlus = true,
            NineteenPlus = false,
            ImportStatus = "ImportOk",
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Completed"
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                Ukprn = 61054789,
                RecognitionNumber = "RN",
                NameLegal = "Legal",
                NameOfqual = "OfqualName",
                NameGovUk = "GovUk",
                Name_Dsi = "DSI",
                Acronym = "ACR"
            },
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = "000013",
                QualificationName = "Qualification Title"
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = "ProcStatusName",
                IsOutcomeDecision = 1
            }
        };

        // Act
        NewQualificationDetailsViewModel vm = response; // implicit operator

        // Assert - a selection of representative properties
        Assert.Equal(response.Id, vm.Id);
        Assert.Equal(response.QualificationId, vm.QualificationId);
        Assert.Equal(response.ProcessStatusId, vm.ProcessStatusId);
        Assert.Equal(response.Status, vm.Status);
        Assert.Equal(response.Type, vm.Type);
        Assert.Equal(response.Ssa, vm.Ssa);
        Assert.Equal(response.Level, vm.Level);
        Assert.Equal(response.Tqt, vm.Tqt);
        Assert.Equal(response.Glh, vm.Glh);
        Assert.Equal(response.OperationalEndDate, vm.OperationalEndDate);
        Assert.Equal(response.RegulationStartDate, vm.RegulationStartDate);
        Assert.Equal(response.Stage.Id, vm.Stage.Id);
        Assert.Equal(response.Stage.Name, vm.Stage.Name);
        Assert.Equal(response.Organisation.NameOfqual, vm.Organisation.NameOfqual);
        Assert.Equal(response.Qual.Qan, vm.Qual.Qan);
        Assert.Equal(response.ProcStatus.Name, vm.ProcStatus.Name);
    }

    [Fact]
    public void MapFundedOffers_AddsFundingDetails_FromFeedbackResponse()
    {
        // Arrange
        var feedback = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>
                {
                    new GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding
                    {
                        FundedOfferName = "Offer A",
                        StartDate = DateOnly.FromDateTime(new DateTime(2023, 1, 1)),
                        EndDate = DateOnly.FromDateTime(new DateTime(2023, 12, 31))
                    },
                    new GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding
                    {
                        FundedOfferName = "Offer B",
                        StartDate = null,
                        EndDate = null
                    }
                }
        };

        var vm = new NewQualificationDetailsViewModel();

        // Act
        vm.MapFundedOffers(feedback);

        // Assert
        Assert.Equal(2, vm.FundingDetails.Count);
        Assert.Equal("Offer A", vm.FundingDetails[0].FundingOfferName);
        Assert.Equal(DateOnly.FromDateTime(new DateTime(2023, 1, 1)), vm.FundingDetails[0].StartDate);
        Assert.Equal(DateOnly.FromDateTime(new DateTime(2023, 12, 31)), vm.FundingDetails[0].EndDate);

        Assert.Equal("Offer B", vm.FundingDetails[1].FundingOfferName);
        Assert.Null(vm.FundingDetails[1].StartDate);
        Assert.Null(vm.FundingDetails[1].EndDate);
    }

    [Fact]
    public void ProcessStatus_ImplicitOperator_ConvertsFromQueryResponseProcessStatus()
    {
        // Arrange
        var source = new GetProcessStatusesQueryResponse.ProcessStatus
        {
            Id = Guid.NewGuid(),
            Name = "Decision Required",
            IsOutcomeDecision = 0
        };

        // Act
        NewQualificationDetailsViewModel.ProcessStatus target = source; // implicit operator

        // Assert
        Assert.Equal(source.Id, target.Id);
        Assert.Equal(source.Name, target.Name);
        Assert.Equal(source.IsOutcomeDecision, target.IsOutcomeDecision);
    }

    [Fact]
    public void AdditionalFormActions_Defaults_AreAsExpected()
    {
        // Arrange & Act
        var vm = new NewQualificationDetailsViewModel();

        // Assert
        Assert.NotNull(vm.AdditionalActions);
        Assert.Equal(string.Empty, vm.AdditionalActions.Note);
        Assert.Null(vm.AdditionalActions.ProcessStatusId);
    }
}
