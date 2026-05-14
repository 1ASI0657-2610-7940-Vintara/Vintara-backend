using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WinesoftPlatform.API.Authentication.application.@internal.commandservices;
using WinesoftPlatform.API.Authentication.application.@internal.queryservices;
using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;

namespace WinesoftPlatform.API.Authentication.interfaces.REST;

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
    [SwaggerOperation(
        Summary = "Register a new user",
        Description = "Creates a new user account with username, email and password")]
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login user",
        Description = "Authenticates user with username or email and returns JWT token")]
    [SwaggerResponse(200, "Login successful", typeof(LoginResponseDto))]
    [SwaggerResponse(401, "Invalid credentials")]
    [SwaggerResponse(400, "Invalid request data")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var (token, user) = await _authQueryService.LoginAsync(request);
            var response = new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            };
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}