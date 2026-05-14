using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Authentication.application.@internal.commandservices;

public class AuthCommandService : IAuthCommandService
{
    private readonly IUserRepository _userRepository;

    public AuthCommandService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task RegisterAsync(RegisterRequestDto request)
    {
        // Check if user with email already exists
        var existingUser = await _userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Check if user with username already exists
        var existingUsername = await _userRepository.FindByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            throw new InvalidOperationException("User with this username already exists");
        }

        // Hash password (using BCrypt for security)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }
}