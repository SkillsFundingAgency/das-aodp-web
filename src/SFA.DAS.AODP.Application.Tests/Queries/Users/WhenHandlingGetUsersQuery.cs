using AutoFixture;
using Moq;
using SFA.DAS.AODP.Application.Queries.Users;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Queries.Users
{
    public class WhenHandlingGetUsersQuery
    {
        private const string ExceptionMessage = "Test exception message";

        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly GetUsersQueryHandler _handler;

        public WhenHandlingGetUsersQuery()
        {
            _handler = new(_apiClient.Object);
        }

        [Fact]
        public async Task Then_The_QueryResult_Is_Returned_As_Expected()
        {
            var expectedResponse = _fixture.Create<GetUsersQueryResponse>();
            var request = _fixture.Create<GetUsersQuery>();

            _apiClient
                .Setup(a => a.Get<GetUsersQueryResponse>(It.IsAny<GetUsersApiRequest>()))
                .ReturnsAsync(expectedResponse);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                _apiClient.Verify(a =>
                    a.Get<GetUsersQueryResponse>(
                        It.IsAny<GetUsersApiRequest>()),
                    Times.Once);

                Assert.NotNull(response);
                Assert.True(response.Success);
                Assert.Null(response.ErrorMessage);
                Assert.NotNull(response.Value);
                Assert.Equal(expectedResponse, response.Value);
            });
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailQueryResult_Is_Returned()
        {
            var request = _fixture.Create<GetUsersQuery>();
            var expectedException = new Exception(ExceptionMessage);

            _apiClient
                .Setup(a => a.Get<GetUsersQueryResponse>(It.IsAny<GetUsersApiRequest>()))
                .ThrowsAsync(expectedException);

            var response = await _handler.Handle(request, default);

            Assert.Multiple(() =>
            {
                Assert.NotNull(response);
                Assert.False(response.Success);
                Assert.NotNull(response.ErrorMessage);
                Assert.NotEmpty(response.ErrorMessage!);
                Assert.Equal(ExceptionMessage, response.ErrorMessage);
                Assert.NotNull(response.Value);
                Assert.Null(response.Value.Users);
            });
        }
    }
}
