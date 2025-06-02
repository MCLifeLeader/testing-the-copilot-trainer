using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyChatApp.ApiService.Controllers;
using MyChatApp.ApiService.Models;
using MyChatApp.Web.Data;
using NSubstitute;
using System.Security.Claims;

namespace MyChatApp.Tests;

[TestFixture]
public class ProfileControllerTests
{
    private ProfileController _controller;
    private UserManager<ApplicationUser> _userManager;
    private ILogger<ProfileController> _logger;
    private ApplicationUser _testUser;

    [SetUp]
    public void Setup()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);
        
        _logger = Substitute.For<ILogger<ProfileController>>();
        _controller = new ProfileController(_userManager, _logger);

        _testUser = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Bio = "This is a test bio",
            AvatarUrl = "/uploads/avatars/test.jpg",
            ProfileVisibility = ProfileVisibility.Public
        };

        // Setup controller context with user claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUser.Id)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _userManager?.Dispose();
    }

    [Test]
    public async Task GetMyProfile_ReturnsUserProfile_WhenUserExists()
    {
        // Arrange
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);

        // Act
        var result = await _controller.GetMyProfileAsync();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var profile = okResult!.Value as UserProfileDto;

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.UserId, Is.EqualTo(_testUser.Id));
        Assert.That(profile.UserName, Is.EqualTo(_testUser.UserName));
        Assert.That(profile.Email, Is.EqualTo(_testUser.Email));
        Assert.That(profile.DisplayName, Is.EqualTo(_testUser.DisplayName));
        Assert.That(profile.Bio, Is.EqualTo(_testUser.Bio));
        Assert.That(profile.AvatarUrl, Is.EqualTo(_testUser.AvatarUrl));
        Assert.That(profile.ProfileVisibility, Is.EqualTo(ProfileVisibilityDto.Public));
    }

    [Test]
    public async Task GetMyProfile_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns((ApplicationUser?)null);

        // Act
        var result = await _controller.GetMyProfileAsync();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task GetUserProfile_ReturnsProfile_WhenPublicProfileRequested()
    {
        // Arrange
        var targetUser = new ApplicationUser
        {
            Id = "target-user-id",
            UserName = "targetuser",
            Email = "target@example.com",
            DisplayName = "Target User",
            Bio = "Target bio",
            ProfileVisibility = ProfileVisibility.Public
        };

        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.FindByIdAsync("target-user-id").Returns(targetUser);

        // Act
        var result = await _controller.GetUserProfileAsync("target-user-id");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var profile = okResult!.Value as UserProfileDto;

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.UserId, Is.EqualTo(targetUser.Id));
        Assert.That(profile.DisplayName, Is.EqualTo(targetUser.DisplayName));
    }

    [Test]
    public async Task GetUserProfile_ReturnsForbidden_WhenPrivateProfileRequested()
    {
        // Arrange
        var targetUser = new ApplicationUser
        {
            Id = "target-user-id",
            ProfileVisibility = ProfileVisibility.Private
        };

        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.FindByIdAsync("target-user-id").Returns(targetUser);

        // Act
        var result = await _controller.GetUserProfileAsync("target-user-id");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task GetUserProfile_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.FindByIdAsync("nonexistent-user-id").Returns((ApplicationUser?)null);

        // Act
        var result = await _controller.GetUserProfileAsync("nonexistent-user-id");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task UpdateMyProfile_UpdatesProfile_WhenValidRequest()
    {
        // Arrange
        var updateRequest = new UpdateUserProfileRequest
        {
            DisplayName = "Updated Name",
            Bio = "Updated bio",
            ProfileVisibility = ProfileVisibilityDto.ContactsOnly
        };

        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.UpdateAsync(_testUser).Returns(IdentityResult.Success);

        // Act
        var result = await _controller.UpdateMyProfileAsync(updateRequest);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var profile = okResult!.Value as UserProfileDto;

        Assert.That(profile, Is.Not.Null);
        Assert.That(profile.DisplayName, Is.EqualTo("Updated Name"));
        Assert.That(profile.Bio, Is.EqualTo("Updated bio"));
        Assert.That(profile.ProfileVisibility, Is.EqualTo(ProfileVisibilityDto.ContactsOnly));

        // Verify user was updated
        Assert.That(_testUser.DisplayName, Is.EqualTo("Updated Name"));
        Assert.That(_testUser.Bio, Is.EqualTo("Updated bio"));
        Assert.That(_testUser.ProfileVisibility, Is.EqualTo(ProfileVisibility.ContactsOnly));
    }

    [Test]
    public async Task UpdateMyProfile_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var updateRequest = new UpdateUserProfileRequest
        {
            DisplayName = "Updated Name"
        };

        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns((ApplicationUser?)null);

        // Act
        var result = await _controller.UpdateMyProfileAsync(updateRequest);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task UpdateMyProfile_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var updateRequest = new UpdateUserProfileRequest
        {
            DisplayName = "Updated Name"
        };

        var identityError = new IdentityError { Description = "Update failed" };
        var failureResult = IdentityResult.Failed(identityError);

        _userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(_testUser);
        _userManager.UpdateAsync(_testUser).Returns(failureResult);

        // Act
        var result = await _controller.UpdateMyProfileAsync(updateRequest);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}