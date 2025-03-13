//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
//using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
//using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
//using SFA.DAS.AODP.Web.Authentication;
//using SFA.DAS.AODP.Web.Enums;
//using SFA.DAS.AODP.Web.Helpers.Markdown;
//using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
//using static SFA.DAS.AODP.Web.Helpers.ListHelper.OrderButtonHelper;
//using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

//namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;

//[Authorize(Policy = PolicyConstants.IsAdminFormsUser)]
//[Area("Admin")]
//public class FormsController : ControllerBase
//{
//    public enum UpdateKeys { FormUpdated, }

//    public FormsController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
//    { }

//    #region Main
//    [Route("/admin/forms")]
//    public async Task<IActionResult> Index()
//    {
//        try
//        {
//            var query = new GetAllFormVersionsQuery();
//            var response = await Send(query);

//            var viewModel = FormVersionListViewModel.Map(response);

//            return View(viewModel);
//        }
//        catch
//        {
//            return Redirect("/Home/Error");
//        }
//    }

using Castle.Core.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using AutoFixture;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;
public class FormControllerTests
{
    private readonly Fixture _fixture = new();
    private readonly FormsController _sut;
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<ILogger<FormsController>> _logger = new();

    public FormControllerTests()
    {
        _sut = new(_mediator.Object, _logger.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewModel()
    {
        // Arrange
        var response = _fixture.Create<BaseMediatrResponse<GetAllFormVersionsQueryResponse>>();
        response.Success = true;

        _mediator.Setup(m => m.Send(It.IsAny<GetAllFormVersionsQuery>(),default))
            .ReturnsAsync(response);

        // Act
        IActionResult actual = await _sut.Index();

        // Assert
        ViewResult? viewResult = actual as ViewResult;
        Assert.NotNull(viewResult);

        FormVersionListViewModel? result = viewResult.ViewData.Model as FormVersionListViewModel;
        Assert.NotNull(result);
    }
//    [Fact]
//    public async Task Index_CreatingDraft()
//    {
//        // Arrange
//        Guid formVersionId = Guid.NewGuid();

//        Mock<FormVersionListViewModel> model = new();
//        model.SetupAdd(s => s.AdditionalActions.CreateDraft.Value += )
//            .Returns(formVersionId);

//        var command = new CreateDraftFormVersionCommand(formVersionId);
//        var response = _fixture.Create<CreateDraftFormVersionCommandResponse>();
//        BaseMediatrResponse<CreateDraftFormVersionCommandResponse> wrapper = new()
//        {
//            Value = response,
//            Success = true
//        };
        
//        _mediator.Setup(m => m.Send(It.IsAny<CreateDraftFormVersionCommand>(), default))
//            .Returns(Task.FromResult(wrapper));
//        // Act
//        IActionResult actual = await _sut.Index(model.Object);

//        // Assert
//        _mediator.Verify(m => m.Send(It.IsAny<CreateDraftFormVersionCommand>(), default), Times.Once());
//        //Assert.Equal(response, model);
//        ViewResult? viewResult = actual as ViewResult;
//        Assert.NotNull(viewResult);

//        FormVersionListViewModel? result = viewResult.ViewData.Model as FormVersionListViewModel;
//        Assert.NotNull(result);

//        //            if (model.AdditionalActions.CreateDraft.HasValue)
//        //            {
//        //                var command = new CreateDraftFormVersionCommand(model.AdditionalActions.CreateDraft.Value);
//        //                var response = await Send(command);
//        //                return RedirectToAction(nameof(Edit), new { formVersionId = response.FormVersionId });

//        //            }
//    }

//    [Fact]
//    public async Task Index_MoveDown()
//    {
//        // Arrange
//        var response = _fixture.Create<BaseMediatrResponse<GetAllFormVersionsQueryResponse>>();
//        response.Success = true;

//        _mediator.Setup(m => m.Send(It.IsAny<GetAllFormVersionsQuery>(), default))
//            .ReturnsAsync(response);

//        // Act
//        IActionResult actual = await _sut.Index();

//        // Assert
//        ViewResult? viewResult = actual as ViewResult;
//        Assert.NotNull(viewResult);

//        FormVersionListViewModel? result = viewResult.ViewData.Model as FormVersionListViewModel;
//        Assert.NotNull(result);
//    }

//    [Fact]
//    public async Task Index_MoveUp()
//    {
//        // Arrange
//        //var response = _fixture.Create<BaseMediatrResponse<CreateDraftFormVersionCommandResponse>>();
//        Guid formVersionId = Guid.NewGuid();

//        var response = _fixture.Create<BaseMediatrResponse<CreateDraftFormVersionCommandResponse>>();
//        response.Success = true;
//        response.Value.FormVersionId = formVersionId;

//        _mediator.Setup(m => m.Send(It.IsAny<CreateDraftFormVersionCommand>(), default))
//            .ReturnsAsync(response);
//        // Act
//        IActionResult actual = await _sut.Index();

//        // Assert
//        ViewResult? viewResult = actual as ViewResult;
//        Assert.NotNull(viewResult);

//        FormVersionListViewModel? result = viewResult.ViewData.Model as FormVersionListViewModel;
//        Assert.NotNull(result);

//        //// Arrange
//        //var request = _fixture.Create<GetAllFormVersionsQuery>();
//        //var response = _fixture.Create<GetAllFormVersionsQueryResponse>();
//        //BaseMediatrResponse<GetAllFormVersionsQueryResponse> wrapper = new()
//        //{
//        //    Value = response,
//        //    Success = true
//        //};

//        //_mediatorMock
//        //    .Setup(m => m.Send(It.IsAny<GetAllFormVersionsQuery>(), default))
//        //    .ReturnsAsync(wrapper);

//        //// Act
//        //var result = await _controller.GetAllAsync();

//        //// Assert
//        //_mediatorMock.Verify(m => m.Send(It.IsAny<GetAllFormVersionsQuery>(), default), Times.Once());
//        //_mediatorMock.Verify(m =>
//        //    m.Send(
//        //        It.IsAny<GetAllFormVersionsQuery>(), default), Times.Once());
//        //var okResult = Assert.IsType<OkObjectResult>(result);
//        //var model = Assert.IsAssignableFrom<GetAllFormVersionsQueryResponse>(okResult.Value);
//        //Assert.Equal(response, model);
//    }
//}


// three unit tests
// create draft
// movedown
// moveup
// type should be of redirecttoaction

//    [Route("/admin/forms")]
//    [HttpPost]
//    public async Task<IActionResult> Index(FormVersionListViewModel model)
//    {
//        try
//        {
//            if (model.AdditionalActions.CreateDraft.HasValue)
//            {
//                var command = new CreateDraftFormVersionCommand(model.AdditionalActions.CreateDraft.Value);
//                var response = await Send(command);
//                return RedirectToAction(nameof(Edit), new { formVersionId = response.FormVersionId });

//            }
//            else if (model.AdditionalActions.MoveDown.HasValue)
//            {
//                var command = new MoveFormDownCommand(model.AdditionalActions.MoveDown.Value);
//                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.FormId.ToString();
//                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();
//                await Send(command);
//            }
//            else if (model.AdditionalActions.MoveUp.HasValue)
//            {
//                var command = new MoveFormUpCommand(model.AdditionalActions.MoveUp.Value);
//                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.FormId.ToString();
//                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();
//                await Send(command);
//            }

//            return RedirectToAction(nameof(Index));
//        }
//        catch
//        {
//            return View(model);
//        }
//    }
//    #endregion

//    #region Create
//    [HttpGet]
//    [Route("/admin/forms/create")]
//    public IActionResult Create()
//    {
//        var viewModel = new CreateFormVersionViewModel();
//        return View(viewModel);
//    }

//    [HttpPost]
//    [Route("/admin/forms/create")]
//    public async Task<IActionResult> Create(CreateFormVersionViewModel viewModel)
//    {
//        try
//        {
//            var command = new CreateFormVersionCommand
//            {
//                Title = viewModel.Name,
//                Description = viewModel.Description,
//            };

//            if (viewModel.AdditionalFormActions.UpdateDescriptionPreview)
//            {
//                viewModel.DescriptionPreview = MarkdownHelper.ToGovUkHtml(viewModel.Description);
//                return View(viewModel);
//            }

//            var response = await Send(command);
//            return RedirectToAction(nameof(Edit), new { formVersionId = response.Id });
//        }
//        catch
//        {
//            return View(viewModel);
//        }
//    }
//    #endregion

//    #region Edit
//    [HttpGet]
//    [Route("/admin/forms/{formVersionId}")]
//    public async Task<IActionResult> Edit(Guid formVersionId)
//    {
//        try
//        {
//            var formVersionQuery = new GetFormVersionByIdQuery(formVersionId);
//            var response = await Send(formVersionQuery);

//            var viewModel = EditFormVersionViewModel.Map(response);

//            ShowNotificationIfKeyExists(UpdateKeys.FormUpdated.ToString(), ViewNotificationMessageType.Success, "The form has been updated.");

//            return View(viewModel);
//        }
//        catch
//        {
//            return Redirect("/Home/Error");
//        }

//    }

//    [HttpPost]
//    [Route("/admin/forms/{formVersionId}")]
//    public async Task<IActionResult> Edit(EditFormVersionViewModel editFormVersionViewModel)
//    {
//        try
//        {
//            if (editFormVersionViewModel.AdditionalFormActions.Publish != default)
//            {
//                var command = new PublishFormVersionCommand(editFormVersionViewModel.Id);
//                await Send(command);

//            }
//            else if (editFormVersionViewModel.AdditionalFormActions.UnPublish != default)
//            {
//                var command = new UnpublishFormVersionCommand(editFormVersionViewModel.Id);
//                await Send(command);
//            }
//            else if (editFormVersionViewModel.AdditionalFormActions.MoveUp != default)
//            {
//                var command = new MoveSectionUpCommand()
//                {
//                    FormVersionId = editFormVersionViewModel.Id,
//                    SectionId = editFormVersionViewModel.AdditionalFormActions.MoveUp ?? Guid.Empty,
//                };
//                await Send(command);

//                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.SectionId.ToString();
//                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Up.ToString();

//            }
//            else if (editFormVersionViewModel.AdditionalFormActions.MoveDown != default)
//            {
//                var command = new MoveSectionDownCommand()
//                {
//                    FormVersionId = editFormVersionViewModel.Id,
//                    SectionId = editFormVersionViewModel.AdditionalFormActions.MoveDown ?? Guid.Empty,
//                };
//                await Send(command);

//                TempData[UpdateTempDataKeys.FocusItemId.ToString()] = command.SectionId.ToString();
//                TempData[UpdateTempDataKeys.Directon.ToString()] = OrderDirection.Down.ToString();

//            }
//            else if (editFormVersionViewModel.AdditionalFormActions.UpdateDescriptionPreview)
//            {

//                var formVersionQuery = new GetFormVersionByIdQuery(editFormVersionViewModel.Id);
//                var response = await Send(formVersionQuery);

//                var viewModel = EditFormVersionViewModel.Map(response);

//                viewModel.Title = editFormVersionViewModel.Title;
//                viewModel.Description = editFormVersionViewModel.Description;
//                viewModel.DescriptionHTML = MarkdownHelper.ToGovUkHtml(viewModel.Description);
//                ViewBag.AutoFocusOnUpdateDescriptionButton = true;



//                return View(viewModel);
//            }
//            else
//            {
//                var command = new UpdateFormVersionCommand()
//                {
//                    FormVersionId = editFormVersionViewModel.Id,
//                    Description = editFormVersionViewModel.Description,
//                    Order = editFormVersionViewModel.Order,
//                    Name = editFormVersionViewModel.Title
//                };
//                await Send(command);

//                TempData[UpdateKeys.FormUpdated.ToString()] = true;
//            }
//            return RedirectToAction(nameof(Edit), new { formVersionId = editFormVersionViewModel.Id });
//        }
//        catch
//        {
//            return View(editFormVersionViewModel);
//        }
//    }
//    #endregion

//    #region Delete
//    [Route("/admin/forms/{formVersionId}/delete")]
//    public async Task<IActionResult> Delete(Guid formVersionId)
//    {
//        try
//        {
//            var query = new GetFormVersionByIdQuery(formVersionId);
//            var response = await Send(query);
//            return View(new DeleteFormViewModel()
//            {
//                FormVersionId = formVersionId,
//                Title = response.Title
//            });
//        }
//        catch
//        {
//            return Redirect("/Home/Error");
//        }
//    }

//    [HttpPost]
//    [Route("/admin/forms/{formVersionId}/delete")]
//    public async Task<IActionResult> DeleteConfirmed(DeleteFormViewModel model)
//    {
//        try
//        {
//            var command = new DeleteFormVersionCommand(model.FormVersionId);
//            await Send(command);
//            return RedirectToAction(nameof(Index));
//        }
//        catch
//        {
//            return View(model);
//        }
//    }
//    #endregion
//}