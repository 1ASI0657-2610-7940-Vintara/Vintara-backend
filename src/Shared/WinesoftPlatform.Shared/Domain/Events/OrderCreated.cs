namespace WinesoftPlatform.Shared.Domain.Events;

public record OrderCreated(int OrderId, int SupplyId, int Quantity, int OwnerId);
