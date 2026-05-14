namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;
/// <summary>
/// Resource representing daily supply rotation metrics.
/// </summary>
/// <param name="Day">The specific date for which the rotation data is calculated.</param>
/// <param name="Movements">The total number of supply movements (entries or updates) recorded on that day.</param>
public record SupplyRotationResource(DateTime Day, int Movements);