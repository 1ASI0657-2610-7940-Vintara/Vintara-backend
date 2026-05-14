namespace WinesoftPlatform.API.Analytics.Domain.Model.Queries;

/// <summary>
/// Query to retrieve inventory Key Performance Indicators Kpis like total costs
/// </summary>
/// <param name="StartDate">The start date of the period</param>
/// <param name="EndDate">The end date of the period</param>
public record GetInventoryKpisQuery(DateTime? StartDate, DateTime? EndDate);