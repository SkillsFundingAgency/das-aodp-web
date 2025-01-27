using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using SFA.DAS.AODP.Web.Controllers;
using SFA.DAS.AODP.Web.Models.Forms;
using SFA.DAS.AODP.Web.Models.Section;

namespace SFA.DAS.AODP.Web.Tests.Controllers
{
    public class SectionsTests : IDisposable
    {
        private Mock<IMediator> _mediatorMock;
        private SectionsController _controller;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new SectionsController(_mediatorMock.Object);
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
            var request = new CreateSectionViewModel();
            var formVersionId = _fixture.Create<Guid>();

            //Act
            var result = await _controller.Create(formVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as CreateSectionViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<CreateSectionViewModel>());
        }

        [Test]
        public async Task Create_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new CreateSectionCommandResponse();
            expectedResponse = _fixture
                .Build<CreateSectionCommandResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new CreateSectionViewModel();
            request = _fixture
                .Build<CreateSectionViewModel>()
                .Create();

            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateSectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Create(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Test]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetSectionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetSectionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetSectionByIdQuery(expectedResponse.Data.Id, expectedResponse.Data.FormVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetSectionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Data.FormVersionId, expectedResponse.Data.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditSectionViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<EditSectionViewModel>());
        }

        [Test]
        public async Task Edit_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new UpdateSectionCommandResponse();
            expectedResponse = _fixture
                .Build<UpdateSectionCommandResponse>()
                .With(w => w.Success, true)
                .Create();
            var request = new EditSectionViewModel();
            request = _fixture
                .Build<EditSectionViewModel>()
                .Create();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateSectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Test]
        public async Task Delete_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetSectionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetSectionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetSectionByIdQuery(expectedResponse.Data.Id, expectedResponse.Data.FormVersionId);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetSectionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Data.Id, expectedResponse.Data.FormVersionId);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeleteSectionViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<DeleteSectionViewModel>());
        }

        [Test]
        public async Task DeleteConfirmed_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new DeleteSectionCommandResponse();
            expectedResponse = _fixture
                .Build<DeleteSectionCommandResponse>()
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
            Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }
    }
}