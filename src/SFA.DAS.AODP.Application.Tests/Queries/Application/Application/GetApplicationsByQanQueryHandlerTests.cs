using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Application.Application;

public class GetApplicationsByQanQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccessAndValue_WhenApiClientReturnsResponse()
    {
        // Arrange
        var qan = "000123";
        var apiResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>
                {
                    new GetApplicationsByQanQueryResponse.Application
                    {
                        Id = Guid.NewGuid(),
                        Name = "App A",
                        CreatedDate = DateTime.UtcNow.AddDays(-5),
                        SubmittedDate = DateTime.UtcNow.AddDays(-2)
                    }
                }
        };

        var mockApiClient = new Mock<IApiClient>();
        mockApiClient
            .Setup(c => c.Get<GetApplicationsByQanQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        var handler = new GetApplicationsByQanQueryHandler(mockApiClient.Object);
        var query = new GetApplicationsByQanQuery(qan);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Same(apiResponse, result.Value);

        mockApiClient.Verify(c => c.Get<GetApplicationsByQanQueryResponse>(
            It.Is<IGetApiRequest>(r => r.GetType() == typeof(GetApplicationsByQanApiRequest)
                                       && ((GetApplicationsByQanApiRequest)r).Qan == qan)
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndNullValue_WhenApiClientReturnsNull()
    {
        // Arrange
        var qan = "000999";
        GetApplicationsByQanQueryResponse? apiResponse = null;

        var mockApiClient = new Mock<IApiClient>();
        mockApiClient
            .Setup(c => c.Get<GetApplicationsByQanQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse!); 

        var handler = new GetApplicationsByQanQueryHandler(mockApiClient.Object);
        var query = new GetApplicationsByQanQuery(qan);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Value);

        mockApiClient.Verify(c => c.Get<GetApplicationsByQanQueryResponse>(
            It.Is<IGetApiRequest>(r => r.GetType() == typeof(GetApplicationsByQanApiRequest)
                                       && ((GetApplicationsByQanApiRequest)r).Qan == qan)
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorMessage_WhenApiClientThrowsException()
    {
        // Arrange
        var qan = "ERR001";
        var expectedMessage = "boom";

        var mockApiClient = new Mock<IApiClient>();
        mockApiClient
            .Setup(c => c.Get<GetApplicationsByQanQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ThrowsAsync(new Exception(expectedMessage));

        var handler = new GetApplicationsByQanQueryHandler(mockApiClient.Object);
        var query = new GetApplicationsByQanQuery(qan);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(expectedMessage, result.ErrorMessage);

        Assert.NotNull(result.Value);
        Assert.Empty(result.Value.Applications);

        mockApiClient.Verify(c => c.Get<GetApplicationsByQanQueryResponse>(
            It.Is<IGetApiRequest>(r => r.GetType() == typeof(GetApplicationsByQanApiRequest)
                                       && ((GetApplicationsByQanApiRequest)r).Qan == qan)
        ), Times.Once);
    }
}
