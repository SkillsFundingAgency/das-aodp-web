using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Mappers;

namespace SFA.DAS.AODP.Web.UnitTests.Mappers;

public class ApplicationMapperTests
{
    [Fact]
    public void Map_ReturnsEmptyList_WhenNoApplications()
    {
        // Arrange
        var response = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>()
        };

        // Act
        var result = ApplicationMapper.Map(response);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsFields_PadsNameAndMapsStatusAndReviewId()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var created = new DateTime(2024, 1, 1);
        var submitted = new DateTime(2024, 01, 02);
        var app = new GetApplicationsByQanQueryResponse.Application
        {
            Id = Guid.NewGuid(),
            CreatedDate = created,
            SubmittedDate = submitted,
            Status = ApplicationStatus.Approved,
            ReferenceId = 123,
            ApplicationReviewId = reviewId
        };

        var response = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application> { app }
        };

        // Act
        var result = ApplicationMapper.Map(response);

        // Assert
        Assert.Single(result);
        var mapped = result[0];
        Assert.Equal(app.Id, mapped.Id);
        Assert.Equal("000123", mapped.Name); 
        Assert.Equal(created, mapped.CreatedDate);
        Assert.Equal(submitted, mapped.SubmittedDate);
        Assert.Equal(ApplicationStatusDisplay.Dictionary[ApplicationStatus.Approved], mapped.Status);
        Assert.Equal(123, mapped.ReferenceId);
        Assert.Equal(reviewId, mapped.ApplicationReviewId);
        Assert.Equal(submitted, mapped.ApplicationDate);
    }

    [Fact]
    public void Map_OrdersBySubmittedDateDescending_WithNullsLast()
    {
        // Arrange
        var earlier = new DateTime(2023, 1, 1);
        var middle = new DateTime(2023, 2, 1);
        var later = new DateTime(2023, 3, 1);

        var appA = new GetApplicationsByQanQueryResponse.Application
        {
            Id = Guid.NewGuid(),
            SubmittedDate = middle,
            CreatedDate = earlier,
            Status = ApplicationStatus.Draft,
            ReferenceId = 1
        };

        var appB = new GetApplicationsByQanQueryResponse.Application
        {
            Id = Guid.NewGuid(),
            SubmittedDate = later,
            CreatedDate = earlier,
            Status = ApplicationStatus.Draft,
            ReferenceId = 2
        };

        var appC = new GetApplicationsByQanQueryResponse.Application
        {
            Id = Guid.NewGuid(),
            SubmittedDate = null, 
            CreatedDate = earlier,
            Status = ApplicationStatus.Draft,
            ReferenceId = 3
        };

        var response = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application> { appC, appA, appB }
        };

        // Act
        var result = ApplicationMapper.Map(response);

        // Assert 
        Assert.Equal(3, result.Count);
        Assert.Equal(appB.Id, result[0].Id);
        Assert.Equal(appA.Id, result[1].Id);
        Assert.Equal(appC.Id, result[2].Id);

        Assert.Equal("000002", result[0].Name);
        Assert.Equal("000001", result[1].Name);
        Assert.Equal("000003", result[2].Name);

        Assert.Equal(later, result[0].ApplicationDate);
        Assert.Equal(middle, result[1].ApplicationDate);
        Assert.Equal(earlier, result[2].ApplicationDate);
    }
}
