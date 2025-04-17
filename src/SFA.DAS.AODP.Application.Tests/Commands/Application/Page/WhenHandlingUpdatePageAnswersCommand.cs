using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using SFA.DAS.AODP.Application.Commands.Application.Page;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Tests.Commands.Application.Page
{
    public class WhenHandlingUpdatePageAnswersCommand
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IApiClient> _apiClient = new();
        private readonly UpdatePageAnswersCommandHandler _handler;


        public WhenHandlingUpdatePageAnswersCommand()
        {
            _handler = new(_apiClient.Object);
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
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
            var expectedResponse = _fixture.Create<UpdatePageAnswersCommandResponse>();
            var request = _fixture.Create<UpdatePageAnswersCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<UpdatePageAnswersApiRequest>()));

            // Act
            var response = await _handler.Handle(request, default);

            // Assert
            _apiClient
                .Verify(a => a.Put(It.Is<UpdatePageAnswersApiRequest>(r => r.Data == request)));

            Assert.NotNull(response);
            Assert.True(response.Success);

            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task And_Api_Errors_Then_The_FailCommandResult_Is_Returned()
        {
            // Arrange
            var expectedException = _fixture.Create<Exception>();
            var request = _fixture.Create<UpdatePageAnswersCommand>();
            _apiClient
                .Setup(a => a.Put(It.IsAny<UpdatePageAnswersApiRequest>()))
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