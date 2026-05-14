namespace WinesoftPlatform.API.Analytics.Domain.Model.Queries;

/// <summary>
/// Query to retrieve supply rotation metrics for a period.
/// </summary>
/// <param name="StartDate">The start date of the period</param>
/// <param name="EndDate">The end date of the period</param>
public record GetSupplyRotationQuery(DateTime? StartDate, DateTime? EndDate);