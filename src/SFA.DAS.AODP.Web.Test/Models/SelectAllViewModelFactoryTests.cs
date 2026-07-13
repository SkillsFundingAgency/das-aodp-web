using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Web.Models.BulkActions;

namespace SFA.DAS.AODP.Web.Tests.Models.BulkActions;

public class SelectAllViewModelFactoryTests
{
    private const int DefaultCurrentPage = 1;
    private const int DefaultRecordsPerPage = 10;

    private const int CurrentPage = 2;
    private const int RecordsPerPage = 25;

    private const string QualificationsController = "Qualifications";
    private const string IndexAction = "Index";
    private const string ReviewArea = "Review";

    private const string Name = "Engineering";
    private const string Organisation = "City & Guilds";
    private const string Qan = "12345678";

    [Fact]
    public void ForQualifications_Sets_Base_Properties()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            currentPage: CurrentPage,
            recordsPerPage: RecordsPerPage,
            controllerName: QualificationsController,
            name: null,
            organisation: null,
            qan: null,
            processStatusIds: null,
            ageGroups: null);

        Assert.Equal(CurrentPage, result.CurrentPage);
        Assert.Equal(RecordsPerPage, result.RecordsPerPage);
        Assert.Equal(QualificationsController, result.Controller);
        Assert.Equal(IndexAction, result.Action);
        Assert.Equal(ReviewArea, result.Area);
        Assert.Empty(result.RouteValues);
    }

    [Fact]
    public void ForQualifications_Adds_Name_When_Provided()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            name: Name,
            organisation: null,
            qan: null,
            processStatusIds: null,
            ageGroups: null);

        Assert.Equal(Name, result.RouteValues["name"]);
    }

    [Fact]
    public void ForQualifications_Adds_Organisation_When_Provided()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            name: null,
            organisation: Organisation,
            qan: null,
            processStatusIds: null,
            ageGroups: null);

        Assert.Equal(Organisation, result.RouteValues["organisation"]);
    }

    [Fact]
    public void ForQualifications_Adds_Qan_When_Provided()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            name: null,
            organisation: null,
            qan: Qan,
            processStatusIds: null,
            ageGroups: null);

        Assert.Equal(Qan, result.RouteValues["qan"]);
    }

    [Fact]
    public void ForQualifications_Adds_ProcessStatusIds_When_Not_Empty()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            null,
            null,
            null,
            ids,
            null);

        Assert.True(result.RouteValues.ContainsKey("processStatusIds"));
        Assert.Equal(ids, result.RouteValues["processStatusIds"]);
    }

    [Fact]
    public void ForQualifications_Does_Not_Add_ProcessStatusIds_When_Empty()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            null,
            null,
            null,
            Enumerable.Empty<Guid>(),
            null);

        Assert.False(result.RouteValues.ContainsKey("processStatusIds"));
    }

    [Fact]
    public void ForQualifications_Adds_AgeGroups_When_Not_Empty()
    {
        var ageGroups = new List<AgeGroup> { AgeGroup.SixteenToEighteen, AgeGroup.EighteenPlus };

        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            null,
            null,
            null,
            null,
            ageGroups);

        Assert.True(result.RouteValues.ContainsKey("ageGroups"));
        Assert.Equal(ageGroups, result.RouteValues["ageGroups"]);
    }

    [Fact]
    public void ForQualifications_Does_Not_Add_AgeGroups_When_Empty()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            null,
            null,
            null,
            null,
            Enumerable.Empty<AgeGroup>());

        Assert.False(result.RouteValues.ContainsKey("ageGroups"));
    }

    [Fact]
    public void ForQualifications_Does_Not_Add_AgeGroups_When_Null()
    {
        var result = SelectAllViewModelFactory.ForQualifications(
            DefaultCurrentPage,
            DefaultRecordsPerPage,
            QualificationsController,
            null,
            null,
            null,
            null,
            null);

        Assert.False(result.RouteValues.ContainsKey("ageGroups"));
    }



}