namespace WinesoftPlatform.API.Analytics.Domain.Model.Commands;

/// <summary>
/// Command to generate an analytics report.
/// </summary>
/// <param name="StartDate">The start date for the report period.</param>
/// <param name="EndDate">The end date for the report period.</param>
/// <param name="Widgets">List of widget identifiers to include in the report.</param>
/// <param name="Language">The language code for report localization.</param>
public record GenerateAnalyticsReportCommand(
    DateTime StartDate,
    DateTime EndDate,
    IEnumerable<string> Widgets,
    string Language
);