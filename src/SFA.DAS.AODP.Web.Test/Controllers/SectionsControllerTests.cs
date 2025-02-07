using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Section;

namespace SFA.DAS.AODP.Web.Tests.Controllers
{
    public class SectionsControllerTests
    {
        // private Mock<IMediator> _mediatorMock;
        // private SectionsController _controller;
        // private readonly Fixture _fixture = new Fixture();

        // [SetUp]
        // public void Setup()
        // {
        //     _mediatorMock = new Mock<IMediator>();
        //     _controller = new SectionsController(_mediatorMock.Object);
        // }

        // [TearDown]
        // public void TearDown()
        // {
        //     _controller?.Dispose();
        // }

        private Mock<IMediator> _mediatorMock = new();
        private SectionsController _controller;
        private readonly Fixture _fixture = new();

        public SectionsControllerTests()
        {
            _controller = new SectionsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Create_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new CreateSectionViewModel();
            var formVersionId = _fixture.Create<Guid>();

            //Act
            var result = await _controller.Create(formVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as CreateSectionViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(result); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<CreateSectionViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<CreateSectionViewModel>());
        }

        [Fact]
        public async Task Create_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<CreateSectionCommandResponse>>()
                .With(w => w.Success, true)
                .Create();

            var request = _fixture
                .Build<CreateSectionViewModel>()
                .Create();

            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateSectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Create(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.True(expectedResponse.Success); // Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Fact]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<GetSectionByIdQueryResponse>>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetSectionByIdQuery(expectedResponse.Value.Id, expectedResponse.Value.FormVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetSectionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Value.FormVersionId, expectedResponse.Value.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditSectionViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<EditSectionViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<EditSectionViewModel>());
        }

        [Fact]
        public async Task Edit_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<UpdateSectionCommandResponse>>()
                .With(w => w.Success, true)
                .Create();
            var request = new EditSectionViewModel();
            request = _fixture
                .Build<EditSectionViewModel>()
                .Create();
            request.AdditionalActions.MoveUp = Guid.Empty;
            request.AdditionalActions.MoveDown = Guid.Empty;

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateSectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(request);
            var okResult = result as RedirectToActionResult;

            //Assert
            Assert.NotNull(okResult);
            Assert.True(expectedResponse.Success); // Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Fact]
        public async Task Delete_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<GetSectionByIdQueryResponse>>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetSectionByIdQuery(expectedResponse.Value.Id, expectedResponse.Value.FormVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetSectionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Value.Id, expectedResponse.Value.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeleteSectionViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<DeleteSectionViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<DeleteSectionViewModel>());
        }

        [Fact]
        public async Task DeleteConfirmed_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<DeleteSectionCommandResponse>>()
                .With(w => w.Success, true)
                .Create();
            var request = new DeleteSectionViewModel();
            request = _fixture
                .Build<DeleteSectionViewModel>()
                .Create();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.DeleteConfirmed(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.True(expectedResponse.Success); // Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        // public void Dispose()
        // {
        //     _controller?.Dispose();
        // }
    }
}