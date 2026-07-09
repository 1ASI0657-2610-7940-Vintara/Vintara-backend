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
        var existingUser = await _userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var existingUsername = await _userRepository.FindByUsernameAsync(request.Username);
        if (existingUsername != null)
            throw new InvalidOperationException("User with this username already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<User> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        // Verificar que email no esté tomado por otro usuario
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var existing = await _userRepository.FindByEmailAsync(request.Email);
            if (existing != null && existing.Id != userId)
                throw new InvalidOperationException("Email is already taken by another user.");
            user.Email = request.Email.Trim();
        }

        if (request.FullName != null) user.FullName = request.FullName.Trim();
        if (request.Phone != null) user.Phone = request.Phone.Trim();

        await _userRepository.SaveChangesAsync();
        return user;
    }
}