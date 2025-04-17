using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Domain.Interfaces;
using AutoFixture.Kernel;

namespace SFA.DAS.AODP.Application.Tests.Commands.Qualifications
{
    public class WhenHandlingCreateQualificationDiscussionHistoryCommand
    {
        private readonly IFixture _fixture;
        private readonly Mock<IApiClient> _apiClientMock;
        private readonly CreateQualificationDiscussionHistoryNoteForFundingOffersCommandHandler _handler;

        public WhenHandlingCreateQualificationDiscussionHistoryCommand()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _apiClientMock = _fixture.Freeze<Mock<IApiClient>>();
            _handler = new CreateQualificationDiscussionHistoryNoteForFundingOffersCommandHandler(_apiClientMock.Object);
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
            var request = _fixture.Create<CreateQualificationDiscussionHistoryNoteForFundingOffersCommand>();
            var expectedResponse = new BaseMediatrResponse<EmptyResponse> { Success = true };
            _apiClientMock
                .Setup(a => a.Put(It.IsAny<CreateQualificationDiscussionHistoryNoteForFundingApiRequest>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _apiClientMock
                .Verify(a => a.Put(It.Is<CreateQualificationDiscussionHistoryNoteForFundingApiRequest>(r => r.QualificationVersionId == request.QualificationVersionId && r.Data == request)), Times.Once);

            Assert.NotNull(response);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<CreateQualificationDiscussionHistoryNoteForFundingOffersCommand>();
            var expectedException = _fixture.Create<Exception>();
            _apiClientMock
                .Setup(a => a.Put(It.IsAny<CreateQualificationDiscussionHistoryNoteForFundingApiRequest>()))
                .ThrowsAsync(expectedException);

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(expectedException.Message, response.ErrorMessage);
        }
    }
}
