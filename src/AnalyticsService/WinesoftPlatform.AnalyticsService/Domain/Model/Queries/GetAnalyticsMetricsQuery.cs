namespace WinesoftPlatform.API.Analytics.Domain.Model.Queries;

/// <summary>
/// Query to retrieve analytics metrics for a specified date range.
/// </summary>
/// <param name="StartDate">Optional start date for the query period.</param>
/// <param name="EndDate">Optional end date for the query period.</param>
public record GetAnalyticsMetricsQuery(DateTime? StartDate, DateTime? EndDate);