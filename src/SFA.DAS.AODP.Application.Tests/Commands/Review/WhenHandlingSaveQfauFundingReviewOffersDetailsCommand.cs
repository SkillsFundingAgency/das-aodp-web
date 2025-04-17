using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Review
{
    public class WhenHandlingSaveQfauFundingReviewOffersDetailsCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly SaveQfauFundingReviewOffersDetailsCommandHandler _handler;


        public WhenHandlingSaveQfauFundingReviewOffersDetailsCommand()
        {
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _handler = new(_apiClient.Object);

        }

        public class DateOnlySpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    return new DateOnly(2023, 1, 1); // a valid date
                }

                return new NoSpecimen();
            }
        }

        [Fact]
        public async Task Then_The_CommandResult_Is_Returned_As_Expected()
        {
            // Arrange
            var expectedResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
            var request = _fixture.Create<SaveQfauFundingReviewOffersDetailsCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<SaveQfauFundingReviewOffersDetailsApiRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.Put(It.Is<SaveQfauFundingReviewOffersDetailsApiRequest>(r => r.Data == request)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<SaveQfauFundingReviewOffersDetailsCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<SaveQfauFundingReviewOffersDetailsApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotEmpty(response.ErrorMessage!);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}