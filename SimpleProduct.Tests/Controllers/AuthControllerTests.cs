using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleProduct.Server.Authentication.Controllers;
using SimpleProduct.Server.Authentication.Dtos;
using SimpleProduct.Server.Authentication.Services;
using Xunit;

namespace SimpleProduct.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    private void SetUserContext(string? userId)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Email, "test@example.com"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Register_ReturnsCreated_WhenValid()
    {
        // Arrange
        var request = new RegisterRequest { Email = "user@test.com", Password = "Password123!" };
        var authResponse = new AuthResponse
        {
            AccessToken = "token123",
            RefreshToken = "refresh123",
            Email = "user@test.com"
        };
        _authServiceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var returned = Assert.IsType<AuthResponse>(createdResult.Value);
        Assert.Equal("token123", returned.AccessToken);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest { Email = "duplicate@test.com", Password = "Password123!" };
        _authServiceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User with email already exists."));

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenArgumentInvalid()
    {
        // Arrange
        var request = new RegisterRequest { Email = "bad@test.com", Password = "short" };
        _authServiceMock.Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Password too weak"));

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Password123!" };
        var authResponse = new AuthResponse
        {
            AccessToken = "valid_access_token",
            RefreshToken = "valid_refresh_token",
            Email = "user@test.com"
        };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("valid_access_token", returned.AccessToken);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "WrongPassword" };
        _authServiceMock.Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenValid()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "valid_refresh" };
        var authResponse = new AuthResponse
        {
            AccessToken = "new_access",
            RefreshToken = "new_refresh"
        };
        _authServiceMock.Setup(s => s.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("new_access", returned.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenTokenIsInvalidOrExpired()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "expired_or_invalid_refresh" };
        _authServiceMock.Setup(s => s.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired refresh token."));

        // Act
        var result = await _controller.RefreshToken(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
    }

    [Fact]
    public async Task Revoke_ReturnsOk_WhenUserIsAuthenticated()
    {
        // Arrange
        SetUserContext("user-id-123");
        _authServiceMock.Setup(s => s.RevokeRefreshTokenAsync("user-id-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Revoke(CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Revoke_ReturnsUnauthorized_WhenUserClaimMissing()
    {
        // Arrange
        SetUserContext(null);

        // Act
        var result = await _controller.Revoke(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        SetUserContext("user-id-123");
        var userInfo = new UserInfoResponse
        {
            Id = "user-id-123",
            Email = "test@example.com",
            Name = "Tester",
            Roles = new List<string> { "User" }
        };
        _authServiceMock.Setup(s => s.GetCurrentUserAsync("user-id-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(userInfo);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<UserInfoResponse>(okResult.Value);
        Assert.Equal("user-id-123", returned.Id);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        SetUserContext("missing-user-id");
        _authServiceMock.Setup(s => s.GetCurrentUserAsync("missing-user-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInfoResponse?)null);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenUserClaimMissing()
    {
        // Arrange
        SetUserContext(null);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
