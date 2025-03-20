using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
    private readonly Mock<ILogger<Web.Controllers.HomeController>> _loggerMock = new();
    private readonly Web.Controllers.HomeController _controller;

    public HomeControllerTests() => _controller = new(_loggerMock.Object,_userHelperServiceMock.Object);

    [Fact]
    public async Task Index_ReturnsApply_WhenUserIsAo()
    {
        // Arrange
        _userHelperServiceMock.Setup(m => m.GetUserType())
                     .Returns(AODP.Models.Users.UserType.AwardingOrganisation);

        // Act
        var result = _controller.Index();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/apply/applications", redirect.Url);
    }

    [Theory]
    [InlineData(UserType.Ofqual)]
    [InlineData(UserType.Qfau)]
    [InlineData(UserType.SkillsEngland)]
    public async Task Index_ReturnsReview_WhenUserIsNotAo(UserType userType)
    {
        // Arrange
        _userHelperServiceMock.Setup(m => m.GetUserType())
                     .Returns(userType);

        // Act
        var result = _controller.Index();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/review", redirect.Url);
    }
}
