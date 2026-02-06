using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.Qualifications;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models;

public class QualificationSearchViewModelTests
{
    [Fact]
    public void Constructor_InitializesCollectionsAndPagination()
    {
        var vm = new QualificationSearchViewModel();

        Assert.NotNull(vm.Qualifications);
        Assert.Empty(vm.Qualifications);
        Assert.NotNull(vm.Pagination);
    }

    [Fact]
    public void Validate_EmptyOrWhitespaceSearchTerm_NoValidationErrors()
    {
        var vm = new QualificationSearchViewModel { SearchTerm = "    " };

        var results = vm.Validate(new ValidationContext(vm)).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_SearchTermShorterThanMin_ReturnsValidationError_WithTrimApplied()
    {
        var vm = new QualificationSearchViewModel { SearchTerm = "  abc  " }; 
        var results = vm.Validate(new ValidationContext(vm)).ToList();

        Assert.Single(results);
        var result = results.Single();
        Assert.Equal($"Search term is {5 - 3} character(s) too short.", result.ErrorMessage);
        Assert.Contains(nameof(QualificationSearchViewModel.SearchTerm), result.MemberNames);
    }

    [Theory]
    [InlineData("abcde")] 
    [InlineData("  abcde  ")]
    [InlineData("abcdef")]
    public void Validate_SearchTermWithMinLengthOrLonger_NoValidationErrors(string term)
    {
        var vm = new QualificationSearchViewModel { SearchTerm = term };

        var results = vm.Validate(new ValidationContext(vm)).ToList();

        Assert.Empty(results);
    }

    [Fact]
    public void Map_MapsFieldsAndStatusNames_WhenProcessStatusMatches()
    {
        // Arrange
        var q1StatusId = Guid.NewGuid();
        var q2StatusId = Guid.NewGuid();

        var response = new GetQualificationsQueryResponse
        {
            TotalRecords = 2,
            Skip = 0,
            Take = 10,
            Qualifications = new List<GetMatchingQualificationsQueryItem>
                {
                    new GetMatchingQualificationsQueryItem
                    {
                        Qan = "QAN1",
                        QualificationName = "Qualification One",
                        Status = q1StatusId
                    },
                    new GetMatchingQualificationsQueryItem
                    {
                        Qan = "QAN2",
                        QualificationName = "Qualification Two",
                        Status = q2StatusId
                    }
                }
        };

        var processStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>
            {
                new GetProcessStatusesQueryResponse.ProcessStatus { Id = q1StatusId, Name = "Approved" },
                new GetProcessStatusesQueryResponse.ProcessStatus { Id = q2StatusId, Name = "Hold" }
            };

        var searchTerm = "search-me";

        // Act
        var vm = QualificationSearchViewModel.Map(response, processStatuses, searchTerm);

        // Assert
        Assert.Equal(searchTerm, vm.SearchTerm);
        Assert.Equal(response.TotalRecords, vm.Pagination.TotalRecords);
        Assert.Equal(2, vm.Qualifications.Count);

        var first = vm.Qualifications.SingleOrDefault(q => q.Qan == "QAN1");
        Assert.NotNull(first);
        Assert.Equal("Qualification One", first.QualificationName);
        Assert.Equal("Approved", first.Status);

        var second = vm.Qualifications.SingleOrDefault(q => q.Qan == "QAN2");
        Assert.NotNull(second);
        Assert.Equal("Qualification Two", second.QualificationName);
        Assert.Equal("Hold", second.Status);
    }

    [Fact]
    public void Map_WhenNoProcessStatusMatch_StatusIsNull()
    {
        // Arrange
        var unknownStatusId = Guid.NewGuid();

        var response = new GetQualificationsQueryResponse
        {
            TotalRecords = 1,
            Skip = 0,
            Take = 10,
            Qualifications = new List<GetMatchingQualificationsQueryItem>
                {
                    new GetMatchingQualificationsQueryItem
                    {
                        Qan = "QANX",
                        QualificationName = "Qualification X",
                        Status = unknownStatusId
                    }
                }
        };

        var processStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>(); 

        // Act
        var vm = QualificationSearchViewModel.Map(response, processStatuses, "");

        // Assert
        Assert.Single(vm.Qualifications);
        Assert.Null(vm.Qualifications[0].Status);
    }

    [Fact]
    public void Moq_IsAvailable_Demonstration()
    {
        var mock = new Mock<object>();
        Assert.NotNull(mock.Object);
    }
}