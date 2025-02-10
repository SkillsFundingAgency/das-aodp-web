using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Web.Controllers;
using SFA.DAS.AODP.Web.Models.Forms;

namespace SFA.DAS.AODP.Web.Tests.Controllers
{
    public class FormsControllerTests : IDisposable
    {
        private Mock<IMediator> _mediatorMock;
        private FormsController _controller;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new FormsController(_mediatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Index_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new GetAllFormVersionsQuery();
            var expectedResponse = new GetAllFormVersionsQueryResponse();
            expectedResponse = _fixture
                .Build<GetAllFormVersionsQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllFormVersionsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Index();

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var okResult = (ViewResult)result;

            var returnedData = okResult.Model as FormVersionListViewModel;

            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData.FormVersions.Count, Is.EqualTo(expectedResponse.Data.Count));
        }

        [Test]
        public void Create_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new CreateFormVersionViewModel();

            //Act
            var result = _controller.Create();
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as CreateFormVersionViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<CreateFormVersionViewModel>());
        }

        [Test]
        public async Task Create_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new CreateFormVersionCommandResponse();
            expectedResponse = _fixture
                .Build<CreateFormVersionCommandResponse>()
                .With(w => w.Success, true)
                .Create();
            var request = new CreateFormVersionViewModel();
            request = _fixture
                .Build<CreateFormVersionViewModel>()
                .Create();

            _mediatorMock.Setup(x => x.Send(It.IsAny<CreateFormVersionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            //simulate the post request
            var result = await _controller.Create(request);

            //var result = _controller.Create(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetFormVersionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetFormVersionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetFormVersionByIdQuery(expectedResponse.Data.Id);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetFormVersionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Data.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditFormVersionViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<EditFormVersionViewModel>());
        }

        [Test]
        public async Task Edit_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new UpdateFormVersionCommandResponse();
            expectedResponse = _fixture
                .Build<UpdateFormVersionCommandResponse>()
                .With(w => w.Success, true)
                .Create();
            var request = new EditFormVersionViewModel();
            request = _fixture
                .Build<EditFormVersionViewModel>()
                .Create();
            _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateFormVersionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

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
            var expectedResponse = new GetFormVersionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetFormVersionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetFormVersionByIdQuery(expectedResponse.Data.Id);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetFormVersionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Data.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeleteFormViewModel;

            //Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(returnedData, Is.Not.Null);
            Assert.That(returnedData, Is.TypeOf<DeleteFormViewModel>());
        }

        [Test]
        public async Task DeleteConfirmed_Post_ValidRequest_RedirectsOk()
        {
            //Arrange
            var expectedResponse = new DeleteFormVersionCommandResponse();
            expectedResponse = _fixture
                .Build<DeleteFormVersionCommandResponse>()
                .With(w => w.Success, true)
                .Create();
            var request = new DeleteFormViewModel();
            request = _fixture
                .Build<DeleteFormViewModel>()
                .Create();
            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteFormVersionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.DeleteConfirmed(request);
            var okResult = (RedirectToActionResult)result;

            //Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(okResult.ActionName, Is.EqualTo("Index"));
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }
    }
}