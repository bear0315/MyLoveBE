using Application.Interfaces;
using Application.Request.User;
using Application.Response.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BKApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with pagination and filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var response = await _userService.GetAllAsync(page, pageSize, role, isActive);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "An error occurred while retrieving users"
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only view their own profile unless they are Admin
                if (currentUserId != id && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse
                    {
                        Success = false,
                        Message = "You do not have permission to view this user"
                    });
                }

                var response = await _userService.GetByIdAsync(id);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var response = await _userService.GetByEmailAsync(email);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var response = await _userService.CreateAsync(request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while creating user"
                });
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Users can only update their own profile unless they are Admin
                if (currentUserId != id && currentUserRole != "Admin")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse
                    {
                        Success = false,
                        Message = "You do not have permission to update this user"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var response = await _userService.UpdateAsync(id, request);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while updating user"
                });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Users can only change their own password
                if (currentUserId != id)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new BaseResponse
                    {
                        Success = false,
                        Message = "You can only change your own password"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var response = await _userService.ChangePasswordAsync(id, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                });
            }
        }

        /// <summary>
        /// Update user status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var response = await _userService.UpdateStatusAsync(id, request);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while updating user status"
                });
            }
        }

        /// <summary>
        /// Delete user (soft delete - Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _userService.DeleteAsync(id);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting user"
                });
            }
        }

        /// <summary>
        /// Get current logged-in user profile
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var response = await _userService.GetByIdAsync(currentUserId);

                if (!response.Success)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse
                    {
                        Success = false,
                        Message = "Invalid input data: " + string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                    });
                }

                var createRequest = new CreateUserRequest
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Password = request.Password,
                    Role = "Customer", 
                   
                };

                var response = await _userService.CreateAsync(createRequest);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Success = false,
                    Message = "An error occurred while registering user"
                });
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        #endregion
    }
}
