using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.Home;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Test.Areas.Review.Controllers
{
    public class HomeControllerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly Web.Areas.Review.Controllers.HomeController _controller;

        public HomeControllerTests() => _controller = new(_userHelperServiceMock.Object);

        [Fact]
        public void Index_GetReturnsViewModel_WhenUserIsQfau()
        {
            // Arrange
            var roles = _fixture.CreateMany<string>().ToList();
            _userHelperServiceMock.Setup(m => m.GetUserType())
                         .Returns(UserType.Qfau);
            _userHelperServiceMock.Setup(m => m.GetUserRoles())
                       .Returns(roles);
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ReviewHomeViewModel>(viewResult.ViewData.Model);
            Assert.NotNull(model);
            Assert.Equal(roles, model.UserRoles);
        }

        [Theory]
        [InlineData(UserType.Ofqual)]
        [InlineData(UserType.SkillsEngland)]
        public void Index_GetReturnsReview_WhenUserIsNotQfau(UserType userType)
        {
            // Arrange
            _userHelperServiceMock.Setup(m => m.GetUserType())
                         .Returns(userType);

            // Act
            var result = _controller.Index();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("ApplicationsReview", redirect.ControllerName);
        }

        [Fact]
        public void Index_PostReturnsViewModel_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("", "");

            // Act
            var result = _controller.Index(new ReviewHomeViewModel());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ReviewHomeViewModel>(viewResult.ViewData.Model);
            Assert.NotNull(model);
        }

        [Theory]
        [InlineData(ReviewHomeViewModel.Options.NewQualifications, "Index", "New")]
        [InlineData(ReviewHomeViewModel.Options.ChangedQualifications, "Index", "Changed")]
        [InlineData(ReviewHomeViewModel.Options.ImportData, "Index", "Import")]
        [InlineData(ReviewHomeViewModel.Options.OutputFile, "Index", "OutputFile")]
        [InlineData(ReviewHomeViewModel.Options.ApplicationsReview, "Index", "ApplicationsReview")]
        [InlineData(ReviewHomeViewModel.Options.FormsManagement, "Index", "Forms")]
        public void Index_PostRedirects_WhenModelStateIsValid(ReviewHomeViewModel.Options option, string actionName, string controllerName)
        {
            // Arrange
            var model = new ReviewHomeViewModel()
            {
                SelectedOption = option,
            };

            // Act
            var result = _controller.Index(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(actionName, redirect.ActionName);
            Assert.Equal(controllerName, redirect.ControllerName);
        }

        [Fact]
        public void Index_PostRedirectsToNotFound_WhenUnknownOptionSelected()
        {
            // Arrange
            var model = new ReviewHomeViewModel();

            // Act
            var result = _controller.Index(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);

        }

    }

}
