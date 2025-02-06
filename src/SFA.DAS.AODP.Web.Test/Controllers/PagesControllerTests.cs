using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;

namespace SFA.DAS.AODP.Web.Tests.Controllers
{
    public class PagesControllerTests
    {
        private Mock<IMediator> _mediatorMock = new();
        private PagesController _controller;
        private readonly Fixture _fixture = new();

        public PagesControllerTests()
        {
            _controller = new PagesController(_mediatorMock.Object);
        }
        // private Mock<IMediator> _mediatorMock;
        // private PagesController _controller;
        // private readonly Fixture _fixture = new Fixture();

        // [SetUp]
        // public void Setup()
        // {
        //     _mediatorMock = new Mock<IMediator>();
        //     _controller = new PagesController(_mediatorMock.Object);
        // }

        // [TearDown]
        // public void TearDown()
        // {
        //     _controller?.Dispose();
        // }

        [Fact]
        public async Task Create_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new CreatePageViewModel();
            var formVersionId = _fixture.Create<Guid>();
            var sectionId = _fixture.Create<Guid>();

            //Act
            var result = await _controller.Create(formVersionId, sectionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as CreatePageViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<CreatePageViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<CreatePageViewModel>());
        }

        [Fact]
        public async Task Create_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<CreatePageCommandResponse>>()
                .With(w => w.Success, true)
                .Create();
            var request = new CreatePageViewModel();
            var formVersionId = _fixture.Create<Guid>();
            var sectionId = _fixture.Create<Guid>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<CreatePageCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Create(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Fact]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<GetPageByIdQueryResponse>>()
                .With(w => w.Success, true)
                .Create();

            var formVersionId = _fixture.Create<Guid>();
            var request = new GetPageByIdQuery(expectedResponse.Value.Id, expectedResponse.Value.SectionId, formVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetPageByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Value.Id, expectedResponse.Value.SectionId, request.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditPageViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<EditPageViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<EditPageViewModel>());
        }

        [Fact]
        public async Task Edit_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<UpdatePageCommandResponse>>()
                .With(w => w.Success, true)
                .Create();
            var request = new EditPageViewModel();
            var formVersionId = _fixture.Create<Guid>();
            var sectionId = _fixture.Create<Guid>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdatePageCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Fact]
        public async Task Delete_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<GetPageByIdQueryResponse>>()
                .With(w => w.Success, true)
                .Create();

            var formVersionId = _fixture.Create<Guid>();
            var request = new GetPageByIdQuery(expectedResponse.Value.Id, expectedResponse.Value.SectionId, formVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetPageByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Value.Id, expectedResponse.Value.SectionId, request.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeletePageViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<DeletePageViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<DeletePageViewModel>());
        }

        [Fact]
        public async Task DeleteConfirmed_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = _fixture
                .Build<BaseMediatrResponse<DeletePageCommandResponse>>()
                .With(w => w.Success, true)
                .Create();
            var request = new DeletePageViewModel();
            request.PageId = _fixture.Create<Guid>();
            request.SectionId = _fixture.Create<Guid>();
            request.FormVersionId = _fixture.Create<Guid>();

            var formVersionId = _fixture.Create<Guid>();
            var sectionId = _fixture.Create<Guid>();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeletePageCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.DeleteConfirmed(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        // public void Dispose()
        // {
        //     _controller?.Dispose();
        // }
    }
}