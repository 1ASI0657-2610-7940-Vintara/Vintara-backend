using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;

namespace WinesoftPlatform.API.Authentication.application.@internal.commandservices;

public interface IAuthCommandService
{
    Task RegisterAsync(RegisterRequestDto request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    Task<User> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
}