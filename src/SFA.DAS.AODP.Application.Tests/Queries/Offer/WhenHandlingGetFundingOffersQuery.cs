using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Queries.Offer
{
    public class WhenHandlingGetFundingOffersQuery
    {
        private const string ExceptionMessage = "Test exception message";

        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetFundingOffersQueryHandler _handler;

        public WhenHandlingGetFundingOffersQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_Unordered_Offers_Are_Returned_Alphabetically_By_Name()
        {
            var zebraId = Guid.NewGuid();
            var alphaId = Guid.NewGuid();
            var middleId = Guid.NewGuid();

            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .ReturnsAsync(new GetFundingOffersQueryResponse
                {
                    Offers =
                    [
                        new() { Id = zebraId, Name = "Zebra offer" },
                        new() { Id = alphaId, Name = "alpha offer" },
                        new() { Id = middleId, Name = "Middle offer" }
                    ]
                });

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(["alpha offer", "Middle offer", "Zebra offer"], response.Value.Offers.Select(o => o.Name));
            Assert.Equal([alphaId, middleId, zebraId], response.Value.Offers.Select(o => o.Id));
        }

        [Fact]
        public async Task Then_Comparison_Is_Case_Insensitive()
        {
            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .ReturnsAsync(new GetFundingOffersQueryResponse
                {
                    Offers =
                    [
                        new() { Id = Guid.NewGuid(), Name = "bravo" },
                        new() { Id = Guid.NewGuid(), Name = "Alpha" },
                        new() { Id = Guid.NewGuid(), Name = "charlie" }
                    ]
                });

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response.Value);
            Assert.Equal(["Alpha", "bravo", "charlie"], response.Value.Offers.Select(o => o.Name));
        }

        [Fact]
        public async Task Then_Empty_Collections_Are_Handled()
        {
            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .ReturnsAsync(new GetFundingOffersQueryResponse
                {
                    Offers = []
                });

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Empty(response.Value.Offers);
        }

        [Fact]
        public async Task Then_Equal_Names_Are_Ordered_By_Id()
        {
            var firstId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var secondId = Guid.Parse("00000000-0000-0000-0000-000000000002");

            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .ReturnsAsync(new GetFundingOffersQueryResponse
                {
                    Offers =
                    [
                        new() { Id = secondId, Name = "Same offer" },
                        new() { Id = firstId, Name = "Same offer" }
                    ]
                });

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response.Value);
            Assert.Equal([firstId, secondId], response.Value.Offers.Select(o => o.Id));
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailQueryResult_Is_Returned()
        {
            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .ThrowsAsync(new Exception(ExceptionMessage));

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(ExceptionMessage, response.ErrorMessage);
            Assert.NotNull(response.Value);
            Assert.Empty(response.Value.Offers);
        }

        [Fact]
        public async Task And_Api_Returns_Null_Then_Null_Response_Behavior_Is_Preserved()
        {
            _apiClient
                .Setup(a => a.Get<GetFundingOffersQueryResponse>(It.IsAny<GetFundingOffersApiRequest>()))
                .Returns(Task.FromResult<GetFundingOffersQueryResponse>(null!));

            var response = await _handler.Handle(new GetFundingOffersQuery(), TestContext.Current.CancellationToken);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Null(response.Value);
        }
    }
}
