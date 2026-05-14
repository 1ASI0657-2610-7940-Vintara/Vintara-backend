using WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;

namespace WinesoftPlatform.API.Authentication.application.@internal.commandservices;

public interface IAuthCommandService
{
    Task RegisterAsync(RegisterRequestDto request);
}