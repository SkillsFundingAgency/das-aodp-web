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

    }

}
