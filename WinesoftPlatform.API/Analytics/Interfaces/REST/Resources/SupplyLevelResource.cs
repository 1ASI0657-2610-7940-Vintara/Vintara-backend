namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource representing the current inventory level of a specific supply item.
/// </summary>
/// <param name="Name">The display name of the supply item, example: "Wine Bottles".</param>
/// <param name="CurrentStock">The total quantity currently available in the inventory.</param>
public record SupplyLevelResource(string Name, int CurrentStock);