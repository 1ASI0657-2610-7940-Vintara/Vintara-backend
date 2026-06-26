namespace WinesoftPlatform.Shared.Domain.Events;

public record SupplyStockChanged(int SupplyId, string SupplyName, int NewQuantity, string Unit, int OwnerId);
