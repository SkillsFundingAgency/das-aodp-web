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
    public class FormsControllerTests
    {
        private Mock<IMediator> _mediatorMock = new();
        private FormsController _controller;
        private readonly Fixture _fixture = new();

        public FormsControllerTests()
        {
            _controller = new FormsController(_mediatorMock.Object);
        }

        [Fact]
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
            Assert.IsType<ViewResult>(result);
            var okResult = (ViewResult)result;

            var returnedData = okResult.Model as FormVersionListViewModel;

            Assert.NotNull(returnedData);
            Assert.Equal(returnedData.FormVersions.Count, expectedResponse.Data.Count);
        }

        [Fact]
        public void Create_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var request = new CreateFormVersionViewModel();

            //Act
            var result = _controller.Create();
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as CreateFormVersionViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<CreateFormVersionViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<CreateFormVersionViewModel>());
        }

        [Fact]
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
            Assert.True(expectedResponse.Success); // Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Index", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Index"));
        }

        [Fact]
        public async Task Edit_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetFormVersionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetFormVersionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetFormVersionByIdQuery(expectedResponse.Data!.Id);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetFormVersionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Edit(expectedResponse.Data.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as EditFormVersionViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<EditFormVersionViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<EditFormVersionViewModel>());
        }

        [Fact]
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
            Assert.True(expectedResponse.Success); // Assert.That(expectedResponse.Success, Is.EqualTo(true));
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Edit", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Edit"));
        }

        [Fact]
        public async Task Delete_Get_ValidRequest_ReturnsOk()
        {
            //Arrange
            var expectedResponse = new GetFormVersionByIdQueryResponse();
            expectedResponse = _fixture
                .Build<GetFormVersionByIdQueryResponse>()
                .With(w => w.Success, true)
                .Create();

            var request = new GetFormVersionByIdQuery(expectedResponse.Data!.Id);

            _mediatorMock.Setup(x => x.Send(It.IsAny<GetFormVersionByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

            //Act
            var result = await _controller.Delete(expectedResponse.Data.Id);
            var okResult = (ViewResult)result;
            var returnedData = okResult.Model as DeleteFormViewModel;

            //Assert
            Assert.IsType<ViewResult>(result); // Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.NotNull(returnedData); // Assert.That(returnedData, Is.Not.Null);
            Assert.IsType<DeleteFormViewModel>(returnedData); // Assert.That(returnedData, Is.TypeOf<DeleteFormViewModel>());
        }

        [Fact]
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
            Assert.IsType<RedirectToActionResult>(result); // Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.Equal("Index", okResult.ActionName); // Assert.That(okResult.ActionName, Is.EqualTo("Index"));
        }
    }
}