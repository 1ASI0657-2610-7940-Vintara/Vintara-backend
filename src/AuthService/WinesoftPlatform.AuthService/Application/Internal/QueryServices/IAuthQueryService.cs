


using WinesoftPlatform.AuthService.Interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;

namespace WinesoftPlatform.AuthService.Application.Internal.QueryServices;

public interface IAuthQueryService
{
    Task<(string token, User user)> LoginAsync(LoginRequestDto request);
    Task<string> LoginServiceAsync(string clientId, string clientSecret);
    Task<User?> GetUserByIdAsync(int id);
}