using System.ComponentModel.DataAnnotations;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

public record CreateStockMovementResource(
    [Required] int SupplyId,
    [Required] [Range(1, int.MaxValue)] int Quantity,
    [Required] string Type, // "ENTRY" or "EXIT"
    string Reason,
    DateTime Date
);
