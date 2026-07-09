using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using WinesoftPlatform.API.Shared.Domain.Model;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.AuthService.Application.Internal.QueryServices;

namespace WinesoftPlatform.AuthService.Tests;

public class AuthQueryServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthQueryService _authQueryService;

    public AuthQueryServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup IConfiguration values required for GenerateJwtToken
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("super_secret_key_vintara_backend_sprint3_testing_only_12345");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("vintara");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("vintara-users");

        _authQueryService = new AuthQueryService(_userRepositoryMock.Object, _configurationMock.Object);
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsTokenWithCorrectUserIdClaim()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Username = "testuser",
            Email = "testuser@winesoft.com",
            PasswordHash = "hashed_password"
        };

        // Act
        var methodInfo = typeof(AuthQueryService).GetMethod("GenerateJwtToken", BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new Xunit.Sdk.XunitException("Method GenerateJwtToken not found on AuthQueryService.");
        }

        var tokenString = (string)methodInfo.Invoke(_authQueryService, new object[] { user })!;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(user.Id.ToString(), nameIdentifierClaim.Value);
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsTokenWithCorrectEmailClaim()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Username = "testuser",
            Email = "testuser@winesoft.com",
            PasswordHash = "hashed_password"
        };

        // Act
        var methodInfo = typeof(AuthQueryService).GetMethod("GenerateJwtToken", BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new Xunit.Sdk.XunitException("Method GenerateJwtToken not found on AuthQueryService.");
        }

        var tokenString = (string)methodInfo.Invoke(_authQueryService, new object[] { user })!;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(user.Email, emailClaim.Value);
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsTokenThatIsNotExpired()
    {
        // Arrange
        var user = new User
        {
            Id = 42,
            Username = "testuser",
            Email = "testuser@winesoft.com",
            PasswordHash = "hashed_password"
        };

        // Act
        var methodInfo = typeof(AuthQueryService).GetMethod("GenerateJwtToken", BindingFlags.NonPublic | BindingFlags.Instance);
        if (methodInfo == null)
        {
            throw new Xunit.Sdk.XunitException("Method GenerateJwtToken not found on AuthQueryService.");
        }

        var tokenString = (string)methodInfo.Invoke(_authQueryService, new object[] { user })!;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        // Expiry should be 7 days from now, so it definitely should be in the future.
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }
}
