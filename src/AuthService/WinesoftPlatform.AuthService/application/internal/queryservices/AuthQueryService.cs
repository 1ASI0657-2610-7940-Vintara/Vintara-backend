using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Authentication.application.@internal.queryservices;

public class AuthQueryService : IAuthQueryService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthQueryService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<(string token, User user)> LoginAsync(LoginRequestDto request)
    {
        // Find user by username or email
        var user = await _userRepository.FindByUsernameOrEmailAsync(request.Username);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        // Generate JWT token
        var token = GenerateJwtToken(user);
        return (token, user);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }
        var jwtIssuer = _configuration["Jwt:Issuer"];
        if (string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT Issuer is not configured.");
        }
        var jwtAudience = _configuration["Jwt:Audience"];
        if (string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("JWT Audience is not configured.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> LoginServiceAsync(string clientId, string clientSecret)
    {
        var expectedClientId = _configuration["ServiceAuth:ClientId"] ?? "iot-simulator";
        var expectedClientSecret = _configuration["ServiceAuth:ClientSecret"] ?? "iot-simulator-secret-key-123456";

        if (clientId != expectedClientId || clientSecret != expectedClientSecret)
        {
            throw new UnauthorizedAccessException("Invalid client credentials");
        }

        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured.");
        }
        var jwtIssuer = _configuration["Jwt:Issuer"];
        if (string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("JWT Issuer is not configured.");
        }
        var jwtAudience = _configuration["Jwt:Audience"];
        if (string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("JWT Audience is not configured.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("client_id", clientId),
            new Claim("role", "service"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}