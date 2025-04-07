﻿using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Queries.Qualifications
{
    public class WhenHandlingGetApplicationReviewSharingStatusByIdQuery
    {
        private readonly IFixture _fixture;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly GetApplicationReviewSharingStatusByIdQueryHandler _handler;

        public WhenHandlingGetApplicationReviewSharingStatusByIdQuery()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = new GetApplicationReviewSharingStatusByIdQueryHandler(_apiClientMock.Object);
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
            var expectedResponse = _fixture.Create<GetApplicationReviewSharingStatusByIdQueryResponse>();
            var request = _fixture.Create<GetApplicationReviewSharingStatusByIdQuery>();
            _apiClientMock
                .Setup(a => a.Get<GetApplicationReviewSharingStatusByIdQueryResponse>(It.IsAny<GetApplicationReviewSharingStatusByIdApiRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _apiClientMock
                .Verify(a => a.Get<GetApplicationReviewSharingStatusByIdQueryResponse>(It.Is<GetApplicationReviewSharingStatusByIdApiRequest>(r => r.ApplicationReviewId == request.ApplicationReviewId)));

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.NotNull(response.Value);
            Assert.Equal(expectedResponse, response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<GetApplicationReviewSharingStatusByIdQuery>();
            _apiClientMock
                .Setup(a => a.Get<GetApplicationReviewSharingStatusByIdQueryResponse>(It.IsAny<GetApplicationReviewSharingStatusByIdApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.NotEmpty(response.ErrorMessage!);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}
