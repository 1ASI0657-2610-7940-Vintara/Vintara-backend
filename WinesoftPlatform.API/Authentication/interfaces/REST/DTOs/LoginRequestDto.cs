using System.ComponentModel.DataAnnotations;

namespace WinesoftPlatform.API.Authentication.interfaces.REST.DTOs;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}