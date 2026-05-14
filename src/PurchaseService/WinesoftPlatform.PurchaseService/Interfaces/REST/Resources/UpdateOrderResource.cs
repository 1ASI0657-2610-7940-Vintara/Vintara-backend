using System.ComponentModel.DataAnnotations;

namespace WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;

public record UpdateOrderResource(
    [Required] int ProductId,
    [Required] string Supplier,
    [Required][Range(1, int.MaxValue)] int Quantity,
    [Required] string Status
);