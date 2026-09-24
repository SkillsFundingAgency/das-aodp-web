using System.Security.Claims;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Services.Files;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Forms;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Apply.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
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
        private readonly Mock<ITempDataDictionary> _tempDataMock = new();
        private readonly ApplicationsController _controller;
        private readonly FileUploadValidator _fileUploadValidator;

        private const string OrgId = "00000000-0000-0000-0000-000000000001";
        private const string UserDisplayName = "Test User";
        private const string UserEmail = "user@test.com";
        private const string QanErrorMessage = "Bad QAN";
        private const string ExceptionMessage = "Exception";

        // Minimal PDF header so any signature-based check in FileUploadValidator passes
        private static readonly byte[] PdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n%test\n%%EOF");

        public ApplicationsControllerTests()
        {
            _fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true
            });
            _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
            _mediatorMock.DefaultValue = DefaultValue.Mock;

            _userHelperMock.Setup(u => u.GetUserOrganisationId()).Returns(OrgId);
            _userHelperMock.Setup(u => u.GetUserDisplayName()).Returns(UserDisplayName);
            _userHelperMock.Setup(u => u.GetUserEmail()).Returns(UserEmail);

            var formBuilderSettings = new FormBuilderSettings
            {
                MaxUploadFileSize = 10,
                UploadFileTypesAllowed = new List<string> { ".xlsx", ".docx", ".pdf" }
            };

            _fileUploadValidator = new FileUploadValidator(formBuilderSettings);

            _controller = new ApplicationsController(
                _mediatorMock.Object,
                _validatorMock.Object,
                _loggerMock.Object,
                _fileServiceMock.Object,
                _userHelperMock.Object,
                _fileUploadValidator)
            {
                TempData = _tempDataMock.Object
            };
        }

        private sealed class DateOnlySpecimenBuilder : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type &&
                    (type == typeof(DateOnly) || type == typeof(DateOnly?)))
                {
                    return DateOnly.FromDateTime(DateTime.UtcNow);
                }

                return new NoSpecimen();
            }
        }

        #region Helpers

        private void SetConsentCookie(bool hasConsentCookie)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("rolecode", "ao_user"),
                new Claim("email", "ao.user@test.com")
            }, "test"));

            if (hasConsentCookie)
            {
                httpContext.Request.Headers.Cookie =
                    $"{ConsentController.GetConsentCookieName(httpContext)}=true";
            }

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetupApplicationStatus(ApplicationStatus status)
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Build<GetApplicationByIdQueryResponse>()
                        .With(r => r.Status, status.ToString())
                        .Create()
                });
        }

        private void SetupFileMetadata(List<FileMetadataDto> files)
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetFileMetadataQueryResponse>
                {
                    Success = true,
                    Value = new GetFileMetadataQueryResponse { Files = files }
                });
        }

        private GetApplicationPageByIdQueryResponse SetupPage(int order, int totalSectionPages)
        {
            var page = _fixture.Build<GetApplicationPageByIdQueryResponse>()
                .With(p => p.Order, order)
                .With(p => p.TotalSectionPages, totalSectionPages)
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationPageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationPageByIdQueryResponse>
                {
                    Success = true,
                    Value = page
                });

            return page;
        }

        private void SetupSavePageAnswers()
        {
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdatePageAnswersCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<UpdatePageAnswersCommandResponse>
                {
                    Success = true,
                    Value = new UpdatePageAnswersCommandResponse()
                });
        }

        private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private ApplicationPageViewModel BuildFileUploadModel(Guid applicationId, Guid sectionId, IFormFile file)
        {
            var model = _fixture.Build<ApplicationPageViewModel>()
                .With(m => m.ApplicationId, applicationId)
                .With(m => m.SectionId, sectionId)
                .Without(m => m.RemoveFile)
                .Create();

            var question = model.Questions.First();
            question.Type = QuestionType.File;
            question.Answer.FormFiles = new List<IFormFile> { file };
            model.Questions = new() { question };

            return model;
        }

        private void VerifyNoUploads()
        {
            _fileServiceMock.Verify(s => s.UploadAsync(
                    It.IsAny<FileCategory>(),
                    It.IsAny<FileContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_ReturnsView_WithListApplicationsViewModel()
        {
            var organisationId = Guid.Parse(OrgId);
            var expectedResponse = new BaseMediatrResponse<GetApplicationsByOrganisationIdQueryResponse>
            {
                Success = true,
                Value = _fixture.Create<GetApplicationsByOrganisationIdQueryResponse>()
            };

            SetConsentCookie(true);

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
        public async Task Index_RedirectsToConsent_WhenConsentCookieIsMissing()
        {
            SetConsentCookie(false);

            var result = await _controller.Index();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Multiple(() =>
            {
                Assert.Equal("Index", redirectResult.ActionName);
                Assert.Equal("Consent", redirectResult.ControllerName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.Equal("Review", redirectResult.RouteValues["area"]);
            });
        }

        #endregion

        #region Create

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
        public async Task Create_Post_NullResponse_ReturnsView()
        {
            var model = _fixture.Build<CreateApplicationViewModel>()
                .With(m => m.Name, "Test App")
                .With(m => m.OrganisationId, Guid.Parse(OrgId))
                .Create();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<CreateApplicationCommandResponse>
                {
                    Success = true,
                    Value = null!
                });

            var result = await _controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.ViewData.Model);
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

            Assert.Equal(model, returnedModel);
        }

        [Fact]
        public async Task Create_Get_ReturnsView_WithModel()
        {
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            var formVersion = _fixture.Create<GetFormVersionByIdQueryResponse>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFormVersionByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetFormVersionByIdQueryResponse>
                {
                    Success = true,
                    Value = formVersion
                });

            var result = await _controller.Create(organisationId, formVersionId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateApplicationViewModel>(view.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(formVersionId, model.FormVersionId);
                Assert.Equal(organisationId, model.OrganisationId);
                Assert.Equal(formVersion.Title, model.FormTitle);
            });
        }

        #endregion

        #region Edit

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
        public async Task Edit_Get_ReturnsView_WithModel()
        {
            var organisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            var application = _fixture.Create<GetApplicationByIdQueryResponse>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationByIdQueryResponse>
                {
                    Success = true,
                    Value = application
                });

            var result = await _controller.Edit(organisationId, applicationId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditApplicationViewModel>(view.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(applicationId, model.ApplicationId);
                Assert.Equal(application.Name, model.Name);
                Assert.Equal(application.QualificationNumber, model.QualificationNumber);
            });
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_Get_ReturnsView_WithModel()
        {
            var applicationId = Guid.NewGuid();
            var application = _fixture.Create<GetApplicationByIdQueryResponse>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationByIdQueryResponse>
                {
                    Success = true,
                    Value = application
                });

            var result = await _controller.Delete(applicationId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DeleteApplicationViewModel>(view.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal(applicationId, model.ApplicationId);
                Assert.Equal(application.Reference, model.ApplicationReference);
                Assert.Equal(application.OrganisationId, model.OrganisationId);
                Assert.Equal(application.Name, model.ApplicationName);
                Assert.Equal(application.FormVersionId, model.FormVersionId);
            });
        }

        [Fact]
        public async Task Delete_Post_Success_SetsTempData_AndRedirectsToIndex()
        {
            var applicationId = Guid.NewGuid();
            var model = _fixture.Create<DeleteApplicationViewModel>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteApplicationCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
                {
                    Success = true,
                    Value = new EmptyResponse()
                });

            var result = await _controller.Delete(model, applicationId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ApplicationsController.Index), redirect.ActionName);

            _tempDataMock.VerifySet(
                t => t[ApplicationsController.UpdateKeys.ApplicationDeletedKey.ToString()] = true,
                Times.Once);
        }

        [Fact]
        public async Task Delete_Post_MediatorThrows_ReturnsView()
        {
            var applicationId = Guid.NewGuid();
            var model = _fixture.Create<DeleteApplicationViewModel>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteApplicationCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(ExceptionMessage));

            var result = await _controller.Delete(model, applicationId);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<DeleteApplicationViewModel>(view.Model);
            Assert.Equal(applicationId, returnedModel.ApplicationId);
        }

        #endregion

        #region Submit / Withdraw

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

            _mediatorMock.Verify(m => m.Send(
                It.Is<SubmitApplicationCommand>(c =>
                    c.ApplicationId == applicationId &&
                    c.SubmittedBy == UserDisplayName &&
                    c.SubmittedByEmail == UserEmail),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Submit_Get_ReturnsView()
        {
            var applicationId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationByIdQueryResponse>()
                });

            var result = await _controller.Submit(applicationId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubmitApplicationViewModel>(view.Model);
            Assert.Equal(applicationId, model.ApplicationId);
        }

        [Fact]
        public async Task SubmitConfirmation_ReturnsView()
        {
            var applicationId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationByIdQueryResponse>()
                });

            var result = await _controller.SubmitConfirmation(applicationId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<SubmitApplicationViewModel>(view.Model);
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

            _mediatorMock.Verify(m => m.Send(
                It.Is<WithdrawApplicationCommand>(c =>
                    c.ApplicationId == applicationId &&
                    c.WithdrawnBy == UserDisplayName &&
                    c.WithdrawnByEmail == UserEmail),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WithdrawConfirmation_ReturnsView()
        {
            var result = _controller.WithdrawConfirmation();
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Forms / Preview / ViewApplication

        [Fact]
        public async Task AvailableFormsAsync_ReturnsView_WithModel()
        {
            var organisationId = Guid.NewGuid();

            var formsResponse = _fixture.Create<GetApplicationFormsQueryResponse>();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationFormsQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationFormsQueryResponse>
                {
                    Success = true,
                    Value = formsResponse
                });

            var result = await _controller.AvailableFormsAsync(organisationId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ListAvailableFormsViewModel>(view.Model);
        }

        [Fact]
        public async Task ApplicationFormPreview_ReturnsView()
        {
            var organisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFormPreviewByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetFormPreviewByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetFormPreviewByIdQueryResponse>()
                });

            var result = await _controller.ApplicationFormPreview(organisationId, applicationId, formVersionId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationFormPreviewViewModel>(view.Model);
        }

        [Fact]
        public async Task ViewApplication_SetsRelatedLinks()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            // Url is needed by RelatedLinksBuilder (uses RouteUrl)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var url = new Mock<IUrlHelper>();
            url.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("/fake-url");
            _controller.Url = url.Object;

            _userHelperMock
                .Setup(u => u.GetUserType())
                .Returns(SFA.DAS.AODP.Models.Users.UserType.AwardingOrganisation);

            // Matching section IDs so Map(...) doesn't throw
            var formsResponse = _fixture.Create<GetApplicationFormByIdQueryResponse>();
            var statusResponse = _fixture.Create<GetApplicationFormStatusByApplicationIdQueryResponse>();

            var sectionId = Guid.NewGuid();

            formsResponse.Sections = new List<GetApplicationFormByIdQueryResponse.Section>
            {
                _fixture.Build<GetApplicationFormByIdQueryResponse.Section>()
                    .With(s => s.Id, sectionId)
                    .With(s => s.Order, 1)
                    .Create()
            };

            statusResponse.Sections = new List<GetApplicationFormStatusByApplicationIdQueryResponse.Section>
            {
                _fixture.Build<GetApplicationFormStatusByApplicationIdQueryResponse.Section>()
                    .With(s => s.SectionId, sectionId)
                    .With(s => s.TotalPages, 1)
                    .With(s => s.SkippedPages, 0) // must be != TotalPages so it isn't skipped
                    .With(s => s.PagesRemaining, 1)
                    .Create()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationFormByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationFormByIdQueryResponse>
                {
                    Success = true,
                    Value = formsResponse
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationFormStatusByApplicationIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationFormStatusByApplicationIdQueryResponse>
                {
                    Success = true,
                    Value = statusResponse
                });

            // Act
            var result = await _controller.ViewApplication(organisationId, applicationId, formVersionId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationFormViewModel>(view.Model);

            Assert.NotNull(model.RelatedLinks);
            Assert.NotEmpty(model.RelatedLinks);
        }

        #endregion

        #region ApplicationPage GET

        [Fact]
        public async Task ApplicationPage_Get_ReturnsView_WithGroupedFiles()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var questionId = Guid.NewGuid();

            SetupFileMetadata(new List<FileMetadataDto>
            {
                new FileMetadataDto
                {
                    FileId = Guid.NewGuid(),
                    QuestionId = questionId,
                    FileName = "test.pdf"
                }
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationPageByIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationPageByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationPageByIdQueryResponse>()
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationPageAnswersByPageIdQuery>(), default))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationPageAnswersByPageIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationPageAnswersByPageIdQueryResponse>()
                });

            SetupApplicationStatus(ApplicationStatus.Draft);

            // Act
            var result = await _controller.ApplicationPage(
                organisationId,
                applicationId,
                sectionId,
                pageOrder: 1,
                formVersionId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationPageViewModel>(view.Model);
            Assert.False(model.IsSubmitted);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetFileMetadataQuery>(q =>
                    q.ApplicationId == applicationId &&
                    q.FileCategories.Contains(FileCategory.QuestionUpload)),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplicationPage_Get_WhenApplicationSubmitted_SetsIsSubmitted()
        {
            SetupFileMetadata(new List<FileMetadataDto>());

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationPageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationPageByIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationPageByIdQueryResponse>()
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationPageAnswersByPageIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationPageAnswersByPageIdQueryResponse>
                {
                    Success = true,
                    Value = _fixture.Create<GetApplicationPageAnswersByPageIdQueryResponse>()
                });

            SetupApplicationStatus(ApplicationStatus.InReview);

            var result = await _controller.ApplicationPage(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), pageOrder: 1, Guid.NewGuid());

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationPageViewModel>(view.Model);
            Assert.True(model.IsSubmitted);
        }

        #endregion

        #region ApplicationPage POST - remove file

        [Fact]
        public async Task ApplicationPage_Post_InvalidRemoveFileGuid_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var model = _fixture.Build<ApplicationPageViewModel>()
                .With(m => m.ApplicationId, applicationId)
                .With(m => m.RemoveFile, "not-a-guid")
                .Create();

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 1, totalSectionPages: 2);

            // Act
            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteFileMetataCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplicationPage_Post_RemoveFileNotFound_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var missingFileId = Guid.NewGuid();

            var model = _fixture.Build<ApplicationPageViewModel>()
                .With(m => m.ApplicationId, applicationId)
                .With(m => m.RemoveFile, missingFileId.ToString())
                .Create();

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>()); // does NOT contain requested file id
            SetupPage(order: 1, totalSectionPages: 2);

            // Act
            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteFileMetataCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplicationPage_Post_WhenApplicationNotDraft_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var model = _fixture.Build<ApplicationPageViewModel>()
                .With(m => m.ApplicationId, applicationId)
                .Create();

            SetupApplicationStatus(ApplicationStatus.InReview);

            // Act
            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestResult>(result);
            VerifyNoUploads();
        }

        [Fact]
        public async Task ApplicationPage_Post_ValidRemoveFile_DeletesFile_AndReturnsView()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var model = _fixture.Build<ApplicationPageViewModel>()
                .With(m => m.ApplicationId, applicationId)
                .With(m => m.SectionId, sectionId)
                .With(m => m.RemoveFile, fileId.ToString())
                .Create();

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>
            {
                new FileMetadataDto
                {
                    FileId = fileId,
                    QuestionId = questionId,
                    FileName = "test.pdf"
                }
            });
            SetupPage(order: 1, totalSectionPages: 2);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteFileMetataCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
                {
                    Success = true,
                    Value = new EmptyResponse()
                });

            // Act
            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationPageViewModel>(view.Model);

            _mediatorMock.Verify(
                m => m.Send(It.Is<DeleteFileMetataCommand>(c => c.FileId == fileId), It.IsAny<CancellationToken>()),
                Times.Once);

            // Removing a file must not validate or save the page
            _validatorMock.Verify(v => v.ValidateApplicationPageAnswers(
                It.IsAny<ModelStateDictionary>(),
                It.IsAny<GetApplicationPageByIdQueryResponse>(),
                It.IsAny<ApplicationPageViewModel>()), Times.Never);
            VerifyNoUploads();
        }

        #endregion

        #region ApplicationPage POST - answers and uploads

        [Fact]
        public async Task ApplicationPage_Post_ValidationFails_ReturnsView_AndDoesNotSave()
        {
            var applicationId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();
            var file = CreateFormFile("evidence.pdf", "application/pdf", PdfBytes);
            var model = BuildFileUploadModel(applicationId, sectionId, file);

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 1, totalSectionPages: 2);

            _validatorMock
                .Setup(v => v.ValidateApplicationPageAnswers(
                    It.IsAny<ModelStateDictionary>(),
                    It.IsAny<GetApplicationPageByIdQueryResponse>(),
                    It.IsAny<ApplicationPageViewModel>()))
                .Callback<ModelStateDictionary, GetApplicationPageByIdQueryResponse, ApplicationPageViewModel>(
                    (modelState, _, _) => modelState.AddModelError("Answer", "Required"));

            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationPageViewModel>(view.Model);
            Assert.False(_controller.ModelState.IsValid);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdatePageAnswersCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            VerifyNoUploads();
        }

        [Fact]
        public async Task ApplicationPage_Post_ValidFile_UploadsFile_AndRedirectsToSectionAtEnd()
        {
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();
            var sectionId = Guid.NewGuid();

            var file = CreateFormFile("evidence.pdf", "application/pdf", PdfBytes);
            var model = BuildFileUploadModel(applicationId, sectionId, file);

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 3, totalSectionPages: 3); // last page => end of section
            SetupSavePageAnswers();

            var result = await _controller.ApplicationPageAsync(model, applicationId, organisationId, formVersionId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Multiple(() =>
            {
                Assert.Equal(nameof(ApplicationsController.ViewApplicationSection), redirect.ActionName);
                Assert.NotNull(redirect.RouteValues);
                Assert.Equal(organisationId, redirect.RouteValues["organisationId"]);
                Assert.Equal(applicationId, redirect.RouteValues["applicationId"]);
                Assert.Equal(sectionId, redirect.RouteValues["sectionId"]);
                Assert.Equal(formVersionId, redirect.RouteValues["formVersionId"]);
            });

            _fileServiceMock.Verify(s => s.UploadAsync(
                    FileCategory.QuestionUpload,
                    It.IsAny<FileContext>(),
                    "evidence.pdf",
                    "application/pdf",
                    It.IsAny<Stream>(),
                    UserDisplayName),
                Times.Once);
        }

        [Fact]
        public async Task ApplicationPage_Post_NoDisplayName_UploadsWithEmptyUploader()
        {
            _userHelperMock.Setup(u => u.GetUserDisplayName()).Returns((string)null!);

            var applicationId = Guid.NewGuid();
            var file = CreateFormFile("evidence.pdf", "application/pdf", PdfBytes);
            var model = BuildFileUploadModel(applicationId, Guid.NewGuid(), file);

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 1, totalSectionPages: 1);
            SetupSavePageAnswers();

            await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            _fileServiceMock.Verify(s => s.UploadAsync(
                    FileCategory.QuestionUpload,
                    It.IsAny<FileContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    string.Empty),
                Times.Once);
        }

        [Fact]
        public async Task ApplicationPage_Post_DisallowedFileType_DoesNotUpload_AndReturnsView()
        {
            var applicationId = Guid.NewGuid();
            var file = CreateFormFile("malware.exe", "application/octet-stream", new byte[] { 0x4D, 0x5A, 0x00 });
            var model = BuildFileUploadModel(applicationId, Guid.NewGuid(), file);

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 1, totalSectionPages: 2);
            SetupSavePageAnswers();

            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            // FileUploadValidator throws; the controller catches, logs and redisplays the page
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationPageViewModel>(view.Model);
            VerifyNoUploads();

            // NOTE: current behaviour - answers are saved before files are validated
            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdatePageAnswersCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplicationPage_Post_FileServiceThrows_ReturnsView()
        {
            var applicationId = Guid.NewGuid();
            var file = CreateFormFile("evidence.pdf", "application/pdf", PdfBytes);
            var model = BuildFileUploadModel(applicationId, Guid.NewGuid(), file);

            SetupApplicationStatus(ApplicationStatus.Draft);
            SetupFileMetadata(new List<FileMetadataDto>());
            SetupPage(order: 1, totalSectionPages: 2);
            SetupSavePageAnswers();

            _fileServiceMock
                .Setup(s => s.UploadAsync(
                    It.IsAny<FileCategory>(),
                    It.IsAny<FileContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new IOException(ExceptionMessage));

            var result = await _controller.ApplicationPageAsync(model, applicationId, Guid.NewGuid(), Guid.NewGuid());

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ApplicationPageViewModel>(view.Model);
        }

        #endregion
    }
}
