using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Controllers;
using SFA.DAS.AODP.Web.Models.Page;
using SFA.DAS.AODP.Web.Models.Section;

namespace SFA.DAS.AODP.Web.Tests.Controllers
{
    public class PagesTests : IDisposable
    {
        private Mock<IMediator> _mediatorMock;
        private PagesController _controller;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new PagesController(_mediatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
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
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<CreatePageViewModel>());
        }

        [Test]
        public async Task Create_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new CreatePageCommandResponse();
            expectedResponse = _fixture
                .Build<CreatePageCommandResponse>()
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
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Test]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetPageByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetPageByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var formVersionId = _fixture.Create<Guid>();
            var request = new GetPageByIdQuery(expectedResponse.Data.Id, expectedResponse.Data.SectionId, formVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetPageByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Data.Id, expectedResponse.Data.SectionId, request.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditPageViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<EditPageViewModel>());
        }

        [Test]
        public async Task Edit_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new UpdatePageCommandResponse();
            expectedResponse = _fixture
                .Build<UpdatePageCommandResponse>()
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
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Test]
        public async Task Delete_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetPageByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetPageByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var formVersionId = _fixture.Create<Guid>();
            var request = new GetPageByIdQuery(expectedResponse.Data.Id, expectedResponse.Data.SectionId, formVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetPageByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Data.Id, expectedResponse.Data.SectionId, request.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeletePageViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<DeletePageViewModel>());
        }

        [Test]
        public async Task DeleteConfirmed_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new DeletePageCommandResponse();
            expectedResponse = _fixture
                .Build<DeletePageCommandResponse>()
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
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }
    }
}