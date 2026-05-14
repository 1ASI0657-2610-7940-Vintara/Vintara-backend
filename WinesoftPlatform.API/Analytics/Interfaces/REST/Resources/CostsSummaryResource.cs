namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource representing the summary of costs for a specific period.
/// </summary>
/// <param name="TotalCost">The total calculated cost of supplies used in orders during the period.</param>
/// <param name="StartDate">The start date of the analyzed period.</param>
/// <param name="EndDate">The end date of the analyzed period.</param>
public record CostsSummaryResource(
    double TotalCost, 
    DateTime StartDate, 
    DateTime EndDate
);

