using WinesoftPlatform.API.Analytics.Domain.Model.Commands;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Services;

namespace WinesoftPlatform.API.Analytics.Application.Internal.CommandServices;

/// <summary>
/// Command service for analytics operations.
/// </summary>
/// <param name="reportBuilder">
/// The <see cref="IAnalyticsReportBuilder"/> report builder service.
/// </param>
public class AnalyticsCommandService(IAnalyticsReportBuilder reportBuilder) : IAnalyticsCommandService
{
    /// <inheritdoc />
    public async Task<byte[]> Handle(GenerateAnalyticsReportCommand command)
    {
        var period = new ReportPeriod(command.StartDate, command.EndDate);
        period.Validate();

        var widgets = command.Widgets
            .Select(w => Enum.Parse<WidgetType>(w, ignoreCase: true))
            .ToList();

        return await reportBuilder.GeneratePdfReportAsync(period, widgets, command.Language);
    }
}