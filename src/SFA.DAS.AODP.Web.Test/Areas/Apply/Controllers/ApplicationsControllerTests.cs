using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Apply.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Apply.Controllers
{
    public class ApplicationsControllerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IApplicationAnswersValidator> _validatorMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();
        private readonly Mock<IUserHelperService> _userHelperMock = new();
        private readonly Mock<ILogger<ApplicationsController>> _loggerMock = new();
        private readonly ApplicationsController _controller;

        private const string OrgId = "00000000-0000-0000-0000-000000000001";
        private const string UserDisplayName = "Test User";
        private const string UserEmail = "user@test.com";
        private const string QanErrorMessage = "Bad QAN";
        private const string ExceptionMessage = "Exception";

        public ApplicationsControllerTests()
        {
            _userHelperMock.Setup(u => u.GetUserOrganisationId()).Returns(OrgId);
            _userHelperMock.Setup(u => u.GetUserDisplayName()).Returns(UserDisplayName);
            _userHelperMock.Setup(u => u.GetUserEmail()).Returns(UserEmail);

            _controller = new ApplicationsController(
                _mediatorMock.Object,
                _validatorMock.Object,
                _loggerMock.Object,
                _fileServiceMock.Object,
                _userHelperMock.Object
            )
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };
        }

        [Fact]
        public async Task Index_ReturnsView_WithListApplicationsViewModel()
        {
            var organisationId = Guid.Parse(OrgId);
            var expectedResponse = new BaseMediatrResponse<GetApplicationsByOrganisationIdQueryResponse>
            {
                Success = true,
                Value = _fixture.Create<GetApplicationsByOrganisationIdQueryResponse>()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationsByOrganisationIdQuery>(), default))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ListApplicationsViewModel>(viewResult.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.NotNull(model);
                Assert.Equal(organisationId, model.OrganisationId);
            });
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToViewApplication()
        {
            var organisationId = Guid.Parse(OrgId);
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            var model = _fixture.Build<CreateApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.OrganisationId, organisationId)
                .With(m => m.FormVersionId, formVersionId)
                .Create();

            var value = _fixture
                .Build<CreateApplicationCommandResponse>()
                .With(v => v.IsQanValid, true)
                .With(v => v.Id, applicationId)
                .Create();

            var commandResponse = _fixture
                .Build<BaseMediatrResponse<CreateApplicationCommandResponse>>()
                .With(r => r.Value, value)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.Create(model);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsController.ViewApplication), redirect.ActionName);
                Assert.NotNull(redirect.RouteValues);
                Assert.Contains("organisationId", redirect.RouteValues.Keys);
                Assert.Contains("applicationId", redirect.RouteValues.Keys);
                Assert.Contains("formVersionId", redirect.RouteValues.Keys);
                Assert.Equal(organisationId, redirect.RouteValues["organisationId"]);
                Assert.Equal(applicationId, redirect.RouteValues["applicationId"]);
                Assert.Equal(formVersionId, redirect.RouteValues["formVersionId"]);
            });
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            var model = _fixture.Create<CreateApplicationViewModel>();
            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<CreateApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
                Assert.False(_controller.ModelState.IsValid);
            });
        }

        [Fact]
        public async Task Create_Post_QanInvalid_ReturnsView_WithModelError()
        {
            var organisationId = Guid.Parse(OrgId);

            var model = _fixture.Build<CreateApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.OrganisationId, organisationId)
                .With(m => m.QualificationNumber, "12345678")
                .Create();

            var value = _fixture
                .Build<CreateApplicationCommandResponse>()
                .With(v => v.IsQanValid, false)
                .With(v => v.QanValidationMessage, QanErrorMessage)
                .With(v => v.Id, Guid.NewGuid())
                .Create();

            var commandResponse = _fixture
                .Build<BaseMediatrResponse<CreateApplicationCommandResponse>>()
                .With(r => r.Value, value)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<CreateApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
                Assert.False(_controller.ModelState.IsValid);
                Assert.True(_controller.ModelState.ContainsKey(nameof(model.QualificationNumber)));
                Assert.Equal(QanErrorMessage, _controller.ModelState[nameof(model.QualificationNumber)]!.Errors.First().ErrorMessage);
            });
        }

        [Fact]
        public async Task Create_Post_MediatorThrows_ReturnsView()
        {
            var organisationId = Guid.Parse(OrgId);

            var model = _fixture.Build<CreateApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.OrganisationId, organisationId)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationCommand>(), default))
                .ThrowsAsync(new Exception(ExceptionMessage));

            var result = await _controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<CreateApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
            });
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToViewApplication()
        {
            var organisationId = Guid.Parse(OrgId);
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            var model = _fixture.Build<EditApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.FormVersionId, formVersionId)
                .Create();

            var value = _fixture
                .Build<EditApplicationCommandResponse>()
                .With(v => v.IsQanValid, true)
                .Create();

            var commandResponse = _fixture
                .Build<BaseMediatrResponse<EditApplicationCommandResponse>>()
                .With(r => r.Value, value)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.Edit(model, applicationId, organisationId);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsController.ViewApplication), redirect.ActionName);
                Assert.NotNull(redirect.RouteValues);
                Assert.Contains("organisationId", redirect.RouteValues.Keys);
                Assert.Contains("applicationId", redirect.RouteValues.Keys);
                Assert.Contains("formVersionId", redirect.RouteValues.Keys);
                Assert.Equal(organisationId, redirect.RouteValues["organisationId"]);
                Assert.Equal(applicationId, redirect.RouteValues["applicationId"]);
                Assert.Equal(formVersionId, redirect.RouteValues["formVersionId"]);
            });
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var organisationId = Guid.Parse(OrgId);
            var applicationId = Guid.NewGuid();

            var model = _fixture.Build<EditApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .Create();

            _controller.ModelState.AddModelError("Name", "Required");

            var result = await _controller.Edit(model, applicationId, organisationId);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<EditApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
                Assert.False(_controller.ModelState.IsValid);
                Assert.Equal(organisationId, returnedModel.OrganisationId);
                Assert.Equal(applicationId, returnedModel.ApplicationId);
            });
        }

        [Fact]
        public async Task Edit_Post_QanInvalid_ReturnsView_WithModelError()
        {
            var organisationId = Guid.Parse(OrgId);
            var applicationId = Guid.NewGuid();

            var model = _fixture.Build<EditApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.QualificationNumber, "12345678")
                .Create();

            var value = _fixture
                .Build<EditApplicationCommandResponse>()
                .With(v => v.IsQanValid, false)
                .With(v => v.QanValidationMessage, QanErrorMessage)
                .Create();

            var commandResponse = _fixture
                .Build<BaseMediatrResponse<EditApplicationCommandResponse>>()
                .With(r => r.Value, value)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.Edit(model, applicationId, organisationId);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<EditApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
                Assert.False(_controller.ModelState.IsValid);
                Assert.Equal(organisationId, returnedModel.OrganisationId);
                Assert.Equal(applicationId, returnedModel.ApplicationId);
                Assert.True(_controller.ModelState.ContainsKey(nameof(model.QualificationNumber)));
                Assert.Equal(QanErrorMessage, _controller.ModelState[nameof(model.QualificationNumber)]!.Errors.First().ErrorMessage);
            });
        }

        [Fact]
        public async Task Edit_Post_MediatorThrows_ReturnsView()
        {
            var organisationId = Guid.Parse(OrgId);
            var applicationId = Guid.NewGuid();

            var model = _fixture.Build<EditApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditApplicationCommand>(), default))
                .ThrowsAsync(new Exception(ExceptionMessage));

            var result = await _controller.Edit(model, applicationId, organisationId);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsAssignableFrom<EditApplicationViewModel>(view.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(model, returnedModel);
                Assert.Equal(organisationId, returnedModel.OrganisationId);
                Assert.Equal(applicationId, returnedModel.ApplicationId);
            });
        }

        [Fact]
        public async Task Submit_Post_Success_RedirectsToConfirmation()
        {
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            var commandResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SubmitApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.Submit(applicationId, organisationId);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsController.SubmitConfirmation), redirect.ActionName);
                Assert.NotNull(redirect.RouteValues);
                Assert.Equal(applicationId, redirect.RouteValues["applicationId"]);
                Assert.Equal(organisationId, redirect.RouteValues["organisationId"]);
            });
        }

        [Fact]
        public async Task Withdraw_Get_ReturnsWithdrawViewModel()
        {
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();

            var expectedResponse = new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>
            {
                Success = true,
                Value = _fixture.Build<GetApplicationMetadataByIdQueryResponse>()
                .With(r => r.OrganisationId, organisationId)
                .With(r => r.FormVersionId, Guid.NewGuid())
                .Create()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMetadataByIdQuery>(), default))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Withdraw(applicationId, organisationId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<WithdrawApplicationViewModel>(viewResult.ViewData.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(applicationId, model.ApplicationId);
                Assert.Equal(organisationId, model.OrganisationId);
                Assert.Equal(expectedResponse.Value.FormVersionId, model.FormVersionId);
            });
        }

        [Fact]
        public async Task Withdraw_Post_Success_RedirectsToConfirmation()
        {
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var commandResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<WithdrawApplicationCommand>(), default))
                .ReturnsAsync(commandResponse);

            var result = await _controller.SubmitWithdraw(applicationId, organisationId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ApplicationsController.WithdrawConfirmation), redirect.ActionName);
        }

        [Fact]
        public void WithdrawConfirmation_ReturnsView()
        {
            var result = _controller.WithdrawConfirmation();
            Assert.IsType<ViewResult>(result);
        }
    }
}
