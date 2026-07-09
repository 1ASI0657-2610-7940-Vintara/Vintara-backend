using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.AuthService.Application.Internal.CommandServices;
using WinesoftPlatform.AuthService.Application.Internal.QueryServices;
using WinesoftPlatform.AuthService.Interfaces.REST.DTOs;

namespace WinesoftPlatform.AuthService.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Authentication Management")]
public class AuthController : ControllerBase
{
    private readonly IAuthCommandService _authCommandService;
    private readonly IAuthQueryService _authQueryService;

    public AuthController(IAuthCommandService authCommandService, IAuthQueryService authQueryService)
    {
        _authCommandService = authCommandService;
        _authQueryService = authQueryService;
    }

    [HttpPost("register")]
    [EnableRateLimiting("AuthRateLimit")]
    [SwaggerOperation(Summary = "Register a new user")]
    [SwaggerResponse(201, "User registered successfully")]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(409, "User with this email or username already exists")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            await _authCommandService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { message = "User registered successfully" });
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthRateLimit")]
    [SwaggerOperation(Summary = "Login user")]
    [SwaggerResponse(200, "Login successful", typeof(LoginResponseDto))]
    [SwaggerResponse(401, "Invalid credentials")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var (token, user) = await _authQueryService.LoginAsync(request);
            return Ok(new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("service-token")]
    [SwaggerOperation(Summary = "Obtain service token")]
    [SwaggerResponse(200, "Authentication successful")]
    [SwaggerResponse(401, "Invalid client credentials")]
    public async Task<IActionResult> ServiceToken([FromBody] ServiceTokenRequestDto request)
    {
        try
        {
            var token = await _authQueryService.LoginServiceAsync(request.ClientId, request.ClientSecret);
            return Ok(new { token });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ------------------------------------------------
    // NEW: Get current user info from JWT
    // ------------------------------------------------
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Get current authenticated user info")]
    [SwaggerResponse(200, "Current user", typeof(UserResponseDto))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Invalid token" });

        var user = await _authQueryService.GetUserByIdAsync(userId.Value);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone
        });
    }

    // ------------------------------------------------
    // NEW: Update current user's profile
    // ------------------------------------------------
    [HttpPut("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Update current user profile")]
    [SwaggerResponse(200, "Profile updated", typeof(UserResponseDto))]
    [SwaggerResponse(400, "Invalid data or email already taken")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequestDto request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Invalid token" });

        try
        {
            var updated = await _authCommandService.UpdateProfileAsync(userId.Value, request);
            return Ok(new UserResponseDto
            {
                Id = updated.Id,
                Username = updated.Username,
                Email = updated.Email,
                FullName = updated.FullName,
                Phone = updated.Phone
            });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    // ------------------------------------------------
    // NEW: Change password
    // ------------------------------------------------
    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("AuthRateLimit")]
    [SwaggerOperation(Summary = "Change current user password")]
    [SwaggerResponse(200, "Password changed successfully")]
    [SwaggerResponse(400, "Invalid data")]
    [SwaggerResponse(401, "Current password is incorrect")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Invalid token" });

        try
        {
            await _authCommandService.ChangePasswordAsync(userId.Value, request);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ------------------------------------------------
    // Helper: extraer userId del JWT
    // ------------------------------------------------
    private int? GetUserIdFromClaims()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("nameid")?.Value
                      ?? User.FindFirst("sub")?.Value;

        return int.TryParse(idClaim, out var id) ? id : null;
    }
}