using WinesoftPlatform.AuthService.Interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;

namespace WinesoftPlatform.AuthService.Application.Internal.CommandServices;

public interface IAuthCommandService
{
    Task RegisterAsync(RegisterRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    Task<User> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
}