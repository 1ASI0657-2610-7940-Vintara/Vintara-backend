


using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;
using WinesoftPlatform.API.Shared.Domain.Model;

namespace WinesoftPlatform.API.Authentication.application.@internal.queryservices;

public interface IAuthQueryService
{
    Task<(string token, User user)> LoginAsync(LoginRequestDto request);
    Task<string> LoginServiceAsync(string clientId, string clientSecret);
    Task<User?> GetUserByIdAsync(int id);
}