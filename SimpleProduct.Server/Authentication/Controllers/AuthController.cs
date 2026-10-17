using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleProduct.Server.Authentication.Dtos;
using SimpleProduct.Server.Authentication.Services;

namespace SimpleProduct.Server.Authentication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Created(string.Empty, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Registration Conflict",
                    Detail = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration Error",
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Authenticates a user and returns JWT access and refresh tokens.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Refreshes access token by validating and rotating the refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid Token",
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Revokes active refresh tokens for the authenticated user (Logout).
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _authService.RevokeRefreshTokenAsync(userId);
            return Ok(new { message = "Tokens successfully revoked." });
        }

        /// <summary>
        /// Returns information for the current authenticated user.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userInfo = await _authService.GetCurrentUserAsync(userId);
            if (userInfo == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "User Not Found",
                    Detail = "Authenticated user could not be found."
                });
            }

            return Ok(userInfo);
        }
    }
}
