using Application.Interfaces;
using Application.Request.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Login response with tokens</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Application.Response.Login.LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Get IP address and user agent
                request.IpAddress = GetClientIpAddress();
                request.DeviceInfo = GetUserAgent();

                var response = await _authService.LoginAsync(request);

                if (!response.Success)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = response.Message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi trong quá trình đăng nhập"
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New tokens</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Application.Response.Login.LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid input data",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Get IP address and user agent
                request.IpAddress = GetClientIpAddress();
                request.DeviceInfo = GetUserAgent();

                var response = await _authService.RefreshTokenAsync(request);

                if (!response.Success)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = response.Message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi trong quá trình token refresh"
                });
            }
        }

        /// <summary>
        /// Logout current user (revoke all tokens)
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid user token"
                    });
                }

                var result = await _authService.LogoutAsync(userId);

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to logout"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Revoke a specific refresh token
        /// </summary>
        /// <param name="request">Token to revoke</param>
        /// <returns>Success status</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Refresh token is required"
                    });
                }

                var result = await _authService.RevokeTokenAsync(request.RefreshToken);

                if (!result)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Failed to revoke token"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Token revoked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred during token revocation"
                });
            }
        }

        /// <summary>
        /// Get current authenticated user info
        /// </summary>
        /// <returns>User information</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var status = User.FindFirst("Status")?.Value;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userId,
                        email,
                        name,
                        role,
                        status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred"
                });
            }
        }

        #region Helper Methods

        private string? GetClientIpAddress()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return Request.Headers["User-Agent"].FirstOrDefault();
        }

        #endregion
    }
}
