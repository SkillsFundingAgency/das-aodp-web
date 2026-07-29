using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Services;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Services;

public class QualificationTimelineHistoryBuilderTests
{
    private const string QualificationReference = "61054902";
    private const string QualificationNameOld = "Old Qualification";
    private const string QualificationNameNew = "New Qualification";
    private const string OrganisationNameOld = "Old Organisation";
    private const string OrganisationNameNew = "New Organisation";
    private const string ChangeTitle = "Change";
    private const string OfqualImportUser = "OFQUAL Import";
    private const string TypeOld = "Type A";
    private const string TypeNew = "Type B";
    private const string GradingTypeOld = "Pass";
    private const string GradingTypeNew = "Merit";
    private const string FailedFields = "Title";

    private const int VersionOne = 1;
    private const int VersionTwo = 2;
    private const int VersionThree = 3;

    private static readonly DateTime VersionOneTimestamp = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionTwoTimestamp = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VersionThreeTimestamp = new(2026, 1, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime OperationalEndDateOld = new(2026, 12, 31, 14, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime OperationalEndDateNew = new(2027, 1, 31, 9, 15, 0, DateTimeKind.Utc);

    private readonly QualificationTimelineHistoryBuilder _builder = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetKeyFieldChanges_WithNullOrWhitespace_ReturnsEmpty(string? input)
    {
        var previous = CreateVersion1();
        var latest = CreateVersion2(versionFieldChanges: input);

        var result = _builder.GetKeyFieldChanges(latest, previous);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetKeyFieldChanges_WithUnknownField_ReturnsEmpty()
    {
        var previous = CreateVersion1();
        var latest = CreateVersion2(versionFieldChanges: "SomeUnknownField");

        var result = _builder.GetKeyFieldChanges(latest, previous);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetKeyFieldChanges_WithWhitespace_ReturnsEntries()
    {
        var previous = CreateVersion1();
        var latest = CreateVersion2(versionFieldChanges: " Title , OrganisationName ");

        var result = _builder.GetKeyFieldChanges(latest, previous);

        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.KeyField.DisplayName == "Title");
        result.ShouldContain(x => x.KeyField.DisplayName == "Organisation name");
    }

    [Theory]
    [MemberData(nameof(RecognisedFieldCases))]
    public void GetKeyFieldChanges_WithRecognisedField_ReturnsExpectedChange(
        string changedField,
        string expectedName,
        string expectedWas,
        string expectedNow)
    {
        var previous = CreateVersion1();
        var latest = CreateVersion2(versionFieldChanges: changedField);

        var result = _builder.GetKeyFieldChanges(latest, previous);

        var change = Assert.Single(result);

        change.KeyField.DisplayName.ShouldBe(expectedName);
        change.Was.ShouldBe(expectedWas);
        change.Now.ShouldBe(expectedNow);
    }

    [Fact]
    public void BuildTimelineEntries_WithNoVersions_ReturnsEmpty()
    {
        var result = _builder.BuildTimelineEntries([]);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTimelineEntries_WithVersionOneOnly_ReturnsEmpty()
    {
        var versionOne = CreateVersion1();

        var result = _builder.BuildTimelineEntries(CreateVersions(versionOne));

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTimelineEntries_WithVersionOneOnly_AndFundingFailure_ReturnsFundingEntry()
    {
        var versionOne = CreateVersion(
            version: VersionOne,
            insertedTimestamp: VersionOneTimestamp,
            qualificationName: QualificationNameNew,
            organisationName: OrganisationNameNew,
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var result = _builder.BuildTimelineEntries(CreateVersions(versionOne));

        var item = Assert.Single(result);

        result.ShouldContain(item);
        item.Title.ShouldBe(ChangeTitle);
        item.UserDisplayName.ShouldBe(OfqualImportUser);
        item.Timestamp.ShouldBe(VersionOneTimestamp);
        item.Notes.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void BuildTimelineEntries_WithMissingIntermediateVersion_DoesNotCreateChangeEntry()
    {
        var versionOne = CreateVersion1();

        var versionThree = CreateVersion(
            version: VersionThree,
            insertedTimestamp: VersionThreeTimestamp,
            qualificationName: QualificationNameNew,
            organisationName: OrganisationNameNew,
            versionFieldChanges: "Title");

        var result = _builder.BuildTimelineEntries(CreateVersions(versionThree, versionOne));

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTimelineEntries_WithUnknownVersionFieldChanges_DoesNotCreateChangeEntry()
    {
        var versionOne = CreateVersion1();
        var versionTwo = CreateVersion2(versionFieldChanges: "UnknownField");

        var result = _builder.BuildTimelineEntries(CreateVersions(versionTwo, versionOne));

        result.ShouldBeEmpty();
    }

    [Fact]
    public void BuildTimelineEntries_WithWhitespaceVersionFieldChanges_DoesNotCreateChangeEntry()
    {
        var versionOne = CreateVersion1();
        var versionTwo = CreateVersion2(versionFieldChanges: "   ");

        var result = _builder.BuildTimelineEntries(CreateVersions(versionTwo, versionOne));

        result.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FundingEntryCases))]
    public void BuildTimelineEntries_FundingEligibility(
        string? failedFields,
        bool? eligible,
        int expectedCount)
    {
        var versionOne = CreateVersion(
            version: VersionOne,
            insertedTimestamp: VersionOneTimestamp,
            qualificationName: QualificationNameNew,
            organisationName: OrganisationNameNew,
            fundingEligibilityFailedFields: failedFields,
            eligibleForFunding: eligible);

        var result = _builder.BuildTimelineEntries(CreateVersions(versionOne));

        result.Count.ShouldBe(expectedCount);

        if (expectedCount == 1)
        {
            var item = result.Single();

            item.Title.ShouldBe(ChangeTitle);
            item.UserDisplayName.ShouldBe(OfqualImportUser);
            item.Timestamp.ShouldBe(VersionOneTimestamp);
            item.Notes.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void BuildTimelineEntries_WithChangesAndFunding_ReturnsBoth()
    {
        var versionOne = CreateVersion1();

        var versionTwo = CreateVersion2(
            versionFieldChanges: "Title",
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var result = _builder.BuildTimelineEntries(CreateVersions(versionTwo, versionOne));

        result.Count.ShouldBe(2);
        result.ShouldAllBe(x => x.Title == ChangeTitle);
        result.ShouldAllBe(x => x.UserDisplayName == OfqualImportUser);

        result.ShouldContain(x =>
            x.Timestamp == VersionTwoTimestamp &&
            x.Notes != null &&
            x.Notes.Contains("Title changed from"));

        result.ShouldContain(x =>
            x.Timestamp == VersionTwoTimestamp &&
            !string.IsNullOrWhiteSpace(x.Notes) &&
            !x.Notes.Contains("Title changed from"));
    }

    [Fact]
    public void BuildTimelineEntries_ReturnsEntriesInExpectedOrder()
    {
        var versionOne = CreateVersion1();

        var versionTwo = CreateVersion2(
            versionFieldChanges: "Title");

        var versionThree = CreateVersion(
            version: VersionThree,
            insertedTimestamp: VersionThreeTimestamp,
            qualificationName: QualificationNameNew,
            organisationName: OrganisationNameNew,
            fundingEligibilityFailedFields: FailedFields,
            eligibleForFunding: false);

        var result = _builder.BuildTimelineEntries(CreateVersions(versionThree, versionTwo, versionOne));

        result.Count.ShouldBe(2);
        result[0].Timestamp.ShouldBe(VersionThreeTimestamp);
        result[1].Timestamp.ShouldBe(VersionTwoTimestamp);
    }

    #region Test Data
    public static IEnumerable<object[]> RecognisedFieldCases()
    {
        yield return ["EligibleForFunding", "Eligibility status", "True", "False"];
        yield return ["OrganisationName", "Organisation name", OrganisationNameOld, OrganisationNameNew];
        yield return ["Title", "Title", QualificationNameOld, QualificationNameNew];
        yield return ["Level", "Level", "level 1", "level 2"];
        yield return ["Type", "Type", TypeOld, TypeNew];
        yield return ["TotalCredits", "Total credits", "10", "20"];
        yield return ["Ssa", "SSA", "ssa 1", "ssa 2"];
        yield return ["GradingType", "Grading type", GradingTypeOld, GradingTypeNew];
        yield return ["OfferedInEngland", "Offered in England", "True", "False"];
        yield return ["IntentionToSeekFundingInEngland", "Intention to seek funding in England", "False", "True"];
        yield return ["PreSixteen", "Pre-sixteen", "False", "True"];
        yield return ["SixteenToEighteen", "Sixteen to eighteen", "True", "False"];
        yield return ["EighteenPlus", "Eighteen plus", "False", "True"];
        yield return ["NineteenPlus", "Nineteen plus", "True", "False"];
        yield return ["Glh", "Guided learning hours (GLH)", "100", "200"];
        yield return ["MinimumGlh", "Minimum GLH", "90", "95"];
        yield return ["Tqt", "Total qualification time (TQT)", "120", "240"];
        yield return ["OperationalEndDate", "Operational end date", "31 December 2026, 02:30:00 pm", "31 January 2027, 09:15:00 am"];
        yield return ["OfferedInternationally", "Offered internationally", "False", "True"];
    }

    public static IEnumerable<object?[]> FundingEntryCases()
    {
        yield return [null, true, 0];
        yield return [null, false, 0];
        yield return ["", false, 0];
        yield return ["   ", false, 0];
        yield return [FailedFields, true, 0];
        yield return [FailedFields, null, 1];
        yield return [FailedFields, false, 1];
    }

    #endregion Test Data


    #region Helper methods
    private static GetQualificationDetailsQueryResponse CreateVersion(
        int version,
        DateTime insertedTimestamp,
        string qualificationName,
        string organisationName,
        string? versionFieldChanges = null,
        string? fundingEligibilityFailedFields = null,
        bool? eligibleForFunding = true,
        string? level = "level 1",
        string? type = TypeOld,
        int? totalCredits = 10,
        string? ssa = "ssa 1",
        string? gradingType = GradingTypeOld,
        bool offeredInEngland = true,
        bool? intentionToSeekFundingInEngland = false,
        bool? preSixteen = false,
        bool? sixteenToEighteen = true,
        bool? eighteenPlus = false,
        bool? nineteenPlus = true,
        int? glh = 100,
        int? minimumGlh = 90,
        int? tqt = 120,
        DateTime? operationalEndDate = null,
        bool? offeredInternationally = false)
    {
        operationalEndDate ??= OperationalEndDateOld;

        return new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            Version = version,
            VersionFieldChanges = versionFieldChanges,
            FundingEligibilityFailedFields = fundingEligibilityFailedFields,
            EligibleForFunding = eligibleForFunding,
            InsertedTimestamp = insertedTimestamp,
            LastUpdatedDate = insertedTimestamp,
            Level = level!,
            Type = type!,
            TotalCredits = totalCredits,
            Ssa = ssa!,
            GradingType = gradingType,
            OfferedInEngland = offeredInEngland,
            IntentionToSeekFundingInEngland = intentionToSeekFundingInEngland,
            PreSixteen = preSixteen,
            SixteenToEighteen = sixteenToEighteen,
            EighteenPlus = eighteenPlus,
            NineteenPlus = nineteenPlus,
            Glh = glh,
            MinimumGlh = minimumGlh,
            Tqt = tqt,
            OperationalEndDate = operationalEndDate,
            OfferedInternationally = offeredInternationally,
            Name = qualificationName,
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = qualificationName,
                Versions = []
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = organisationName
            }
        };
    }

    private static List<GetQualificationDetailsQueryResponse> CreateVersions(
        params GetQualificationDetailsQueryResponse[] versions)
    {
        return versions.ToList();
    }

    private static GetQualificationDetailsQueryResponse CreateVersion1()
    {
        return CreateVersion(
            version: VersionOne,
            insertedTimestamp: VersionOneTimestamp,
            qualificationName: QualificationNameOld,
            organisationName: OrganisationNameOld,
            eligibleForFunding: true,
            level: "level 1",
            type: TypeOld,
            totalCredits: 10,
            ssa: "ssa 1",
            gradingType: GradingTypeOld,
            offeredInEngland: true,
            intentionToSeekFundingInEngland: false,
            preSixteen: false,
            sixteenToEighteen: true,
            eighteenPlus: false,
            nineteenPlus: true,
            glh: 100,
            minimumGlh: 90,
            tqt: 120,
            operationalEndDate: OperationalEndDateOld,
            offeredInternationally: false);
    }

    private static GetQualificationDetailsQueryResponse CreateVersion2(
        string? versionFieldChanges = null,
        string? fundingEligibilityFailedFields = null,
        bool? eligibleForFunding = false)
    {
        return CreateVersion(
            version: VersionTwo,
            insertedTimestamp: VersionTwoTimestamp,
            qualificationName: QualificationNameNew,
            organisationName: OrganisationNameNew,
            versionFieldChanges: versionFieldChanges,
            fundingEligibilityFailedFields: fundingEligibilityFailedFields,
            eligibleForFunding: eligibleForFunding,
            level: "level 2",
            type: TypeNew,
            totalCredits: 20,
            ssa: "ssa 2",
            gradingType: GradingTypeNew,
            offeredInEngland: false,
            intentionToSeekFundingInEngland: true,
            preSixteen: true,
            sixteenToEighteen: false,
            eighteenPlus: true,
            nineteenPlus: false,
            glh: 200,
            minimumGlh: 95,
            tqt: 240,
            operationalEndDate: OperationalEndDateNew,
            offeredInternationally: true);
    }

    #endregion Helper methods
}

