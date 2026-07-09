using System.ComponentModel.DataAnnotations;

namespace WinesoftPlatform.AuthService.Interfaces.REST.DTOs;

public class ServiceTokenRequestDto
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;
}
