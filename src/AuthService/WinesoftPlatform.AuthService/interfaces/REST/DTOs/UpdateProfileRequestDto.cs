namespace WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;

public class UpdateProfileRequestDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}