using System.ComponentModel.DataAnnotations;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource for generating an analytics report
/// </summary>
/// <param name="StartDate">The start date of the analyzed period.</param>
/// <param name="EndDate">The end date of the analyzed period.</param>
/// <param name="Widgets">List of widget names to include</param>
public record GenerateReportResource(
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] IEnumerable<string> Widgets,
    string Language = "en"
);