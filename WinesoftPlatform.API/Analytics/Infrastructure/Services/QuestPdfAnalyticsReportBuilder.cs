using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WinesoftPlatform.API.Analytics.Domain.Model.Aggregates;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Resources;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace WinesoftPlatform.API.Analytics.Infrastructure.Services;

/// <summary>
/// QuestPDF implementation of the analytics report builder.
/// </summary>
public class QuestPdfAnalyticsReportBuilder(AppDbContext context, IStringLocalizer<ReportMessages> localizer) 
    : IAnalyticsReportBuilder
{
    private static readonly Color DarkWineColor = Color.FromHex("#4A148C");
    private static readonly Color MediumPurpleColor = Color.FromHex("#6A1B9A");
    private static readonly Color AlertPinkColor = Color.FromHex("#D81B60");
    private static readonly Color TextoOscuro = Colors.Grey.Darken3;
    
    private const int LowStockThreshold = 30;
    public async Task<byte[]> GeneratePdfReportAsync(ReportPeriod period, IEnumerable<WidgetType> widgets, string language)
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        var originalCultureGen = CultureInfo.CurrentCulture;

        try
        {
            var targetCulture = new CultureInfo(string.IsNullOrEmpty(language) ? "en" : language);
            CultureInfo.CurrentUICulture = targetCulture;
            CultureInfo.CurrentCulture = targetCulture;

            var widgetList = widgets.ToList();
            
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextoOscuro));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, period, widgetList));
                    
                    page.Footer().AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium))
                        .Text(text =>
                        {
                            text.Span($"{localizer["Page"]} ");
                            text.CurrentPageNumber();
                            text.Span($" {localizer["Of"]} ");
                            text.TotalPages();
                        });
                });
            });

            return await Task.FromResult(document.GeneratePdf());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
            CultureInfo.CurrentCulture = originalCultureGen;
        }
    }

    /// <summary>
    /// Composes the header section of the report.
    /// </summary>
    /// <param name="container">The container to compose the header in.</param>
    private void ComposeHeader(IContainer container)
    {
        container.Background(DarkWineColor).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(localizer["PlatformName"].Value)
                    .FontSize(24).Bold().FontColor(Colors.White);
                
                column.Item().PaddingTop(5).Text(localizer["ReportTitle"].Value)
                    .FontSize(14).FontColor(Colors.Grey.Lighten2);
            });

            row.ConstantItem(120).AlignRight().AlignMiddle().Column(column =>
            {
                column.Item().Text($"{localizer["GeneratedOn"]}: {DateTime.UtcNow:yyyy-MM-dd}")
                    .FontSize(9).FontColor(Colors.White);
            });
        });
    }

    /// <summary>
    /// Composes the main content section of the report.
    /// </summary>
    /// <param name="container">The container to compose the content in.</param>
    /// <param name="period">The report period.</param>
    /// <param name="widgets">The list of widgets to include.</param>
    private void ComposeContent(IContainer container, ReportPeriod period, List<WidgetType> widgets)
    {
        container.PaddingVertical(15).Column(column =>
        {
            column.Spacing(5);

            column.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
            {
                row.RelativeItem().Text($"{localizer["ReportPeriod"]}: {period.StartDate:yyyy-MM-dd} {localizer["To"]} {period.EndDate:yyyy-MM-dd}")
                    .FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                row.ConstantItem(150).AlignRight().Text($"{localizer["Duration"]}: {period.DaysDuration} {localizer["Days"]}")
                    .FontSize(10).FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            foreach (var widget in widgets)
            {
                column.Item().PaddingTop(15).Element(c => ComposeWidget(c, widget, period));
            }
        });
    }

    /// <summary>
    /// Composes a single widget section.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    /// <param name="widget">The widget type to compose.</param>
    /// <param name="period">The report period.</param>
    private void ComposeWidget(IContainer container, WidgetType widget, ReportPeriod period)
    {
        container.Column(column =>
        {
            var titleKey = $"Widget_{widget}";
            column.Item().Background(MediumPurpleColor).Padding(8).Text(localizer[titleKey].Value)
                .FontSize(13).Bold().FontColor(Colors.White);
            
            column.Item().PaddingTop(8).Element(c => ComposeWidgetContent(c, widget, period));
            
            column.Item().PaddingTop(12).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
        });
    }

    /// <summary>
    /// Routes widget content composition based on widget type.
    /// </summary>
    /// <param name="container">The container to compose the widget content in.</param>
    /// <param name="widget">The widget type.</param>
    /// <param name="period">The report period.</param>
    private void ComposeWidgetContent(IContainer container, WidgetType widget, ReportPeriod period)
    {
        switch (widget)
        {
            case WidgetType.PurchaseOrders:
                ComposePurchaseOrdersWidget(container, period);
                break;
            case WidgetType.SupplyLevels:
                ComposeSupplyLevelsWidget(container);
                break;
            case WidgetType.LowStockAlerts:
                ComposeLowStockAlertsWidget(container);
                break;
            case WidgetType.SupplyRotation:
                ComposeSupplyRotationWidget(container, period);
                break;
            case WidgetType.CostsSummary:
                ComposeCostsSummaryWidget(container, period);
                break;
        }
    }

    /// <summary>
    /// Composes the purchase orders widget content.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    /// <param name="period">The report period.</param>
    private void ComposePurchaseOrdersWidget(IContainer container, ReportPeriod period)
    {
        var orders = context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedDate >= period.StartDate && o.CreatedDate <= period.EndDate)
            .OrderByDescending(o => o.CreatedDate)
            .Take(20)
            .ToList();

        if (!orders.Any())
        {
            container.Padding(10).Text(localizer["NoOrders"].Value)
                .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.ConstantColumn(80);
                columns.RelativeColumn(2);
                columns.ConstantColumn(70);
                columns.ConstantColumn(80);
                columns.ConstantColumn(85);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_ID"].Value);
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Product"].Value);
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Supplier"].Value);
                header.Cell().Element(HeaderCellStyle).AlignRight().Text(localizer["Col_Qty"].Value);
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Status"].Value);
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Date"].Value);
            });

            foreach (var order in orders)
            {
                table.Cell().Element(DataCellStyle).Text(order.Id.ToString());
                table.Cell().Element(DataCellStyle).Text($"#{order.ProductId}");
                table.Cell().Element(DataCellStyle).Text(order.Supplier);
                table.Cell().Element(DataCellStyle).AlignRight().Text(order.Quantity.ToString("N0"));
                table.Cell().Element(DataCellStyle).Text(t => t.Span(order.Status).FontSize(9));
                table.Cell().Element(DataCellStyle).Text(order.CreatedDate?.DateTime.ToString("yyyy-MM-dd") ?? "N/A");
            }
        });
    }

    /// <summary>
    /// Composes the supply levels widget content.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    private void ComposeSupplyLevelsWidget(IContainer container)
    {
        var supplies = context.Supplies
            .AsNoTracking()
            .GroupBy(s => s.SupplyName)
            .Select(g => new { Name = g.Key, TotalQuantity = g.Sum(s => s.Quantity) })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(20)
            .ToList();

        if (!supplies.Any())
        {
            container.Padding(10).Text(localizer["NoSupplies"].Value)
                .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.ConstantColumn(80);
                columns.ConstantColumn(70);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Name"].Value);
                header.Cell().Element(HeaderCellStyle).AlignRight().Text(localizer["Col_Qty"].Value);
                header.Cell().Element(HeaderCellStyle).AlignCenter().Text(localizer["Col_Status"].Value);
            });

            foreach (var supply in supplies)
            {
                bool isLowStock = supply.TotalQuantity < LowStockThreshold;
                var textColor = isLowStock ? AlertPinkColor : TextoOscuro;

                table.Cell().Element(DataCellStyle).Text(supply.Name).FontColor(textColor);
                table.Cell().Element(DataCellStyle).AlignRight().Text(supply.TotalQuantity.ToString("N0")).FontColor(textColor);
                
                table.Cell().Element(DataCellStyle).AlignCenter().Text(isLowStock ? "LOW" : "OK")
                    .FontSize(9).Bold().FontColor(isLowStock ? AlertPinkColor : Colors.Green.Darken1);
            }
        });
    }

    /// <summary>
    /// Composes the low stock alerts widget content.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    private void ComposeLowStockAlertsWidget(IContainer container)
    {
        var alerts = context.Supplies
            .AsNoTracking()
            .Where(s => s.Quantity < LowStockThreshold)
            .Select(s => new { s.SupplyName, s.Quantity })
            .OrderBy(s => s.Quantity)
            .Take(15)
            .ToList();

        if (!alerts.Any())
        {
            container.Padding(10).Background(Colors.Green.Lighten4).Row(row =>
            {
                row.AutoItem().PaddingRight(8).Text("✓").FontSize(14).FontColor(Colors.Green.Darken2);
                row.RelativeItem().Text(localizer["NoAlerts"].Value).FontSize(10).FontColor(Colors.Green.Darken2);
            });
            return;
        }

        container.Column(column =>
        {
            column.Item().Background(AlertPinkColor).Padding(6)
                .Text($" {alerts.Count} {localizer["AlertHeader"]} ({LowStockThreshold})")
                .FontSize(10).Bold().FontColor(Colors.White);

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Product"].Value);
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text(localizer["Col_Current"].Value);
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text(localizer["Col_Threshold"].Value);
                });

                foreach (var alert in alerts)
                {
                    table.Cell().Element(DataCellStyle).Text(alert.SupplyName).FontColor(AlertPinkColor);
                    table.Cell().Element(DataCellStyle).AlignRight().Text(alert.Quantity.ToString()).Bold().FontColor(AlertPinkColor);
                    table.Cell().Element(DataCellStyle).AlignRight().Text(LowStockThreshold.ToString()).FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    /// <summary>
    /// Composes the supply rotation widget content.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    /// <param name="period">The report period.</param>
    private void ComposeSupplyRotationWidget(IContainer container, ReportPeriod period)
    {
        var rotation = context.Supplies
            .AsNoTracking()
            .Where(s => s.Date >= period.StartDate && s.Date <= period.EndDate)
            .GroupBy(s => s.Date.Date)
            .Select(g => new { Date = g.Key, Movements = g.Count() })
            .OrderByDescending(x => x.Date)
            .Take(15)
            .ToList();

        if (!rotation.Any())
        {
            container.Padding(10).Text(localizer["NoRotation"].Value)
                .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
            return;
        }

        var totalMovements = rotation.Sum(r => r.Movements);

        container.Column(column =>
        {
            column.Item().Background(Colors.Blue.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Text($"{localizer["TotalDays"]}: {rotation.Count}").FontSize(10).SemiBold();
                row.ConstantItem(150).AlignRight().Text($"{localizer["TotalMoves"]}: {totalMovements}")
                    .FontSize(10).SemiBold().FontColor(Colors.Blue.Darken2);
            });

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Date"].Value);
                    header.Cell().Element(HeaderCellStyle).Text(localizer["Col_Activity"].Value);
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text(localizer["Col_Movements"].Value);
                });

                var maxMovements = rotation.Max(r => r.Movements);

                foreach (var item in rotation)
                {
                    var barWidth = maxMovements > 0 ? (float)item.Movements / maxMovements : 0;

                    table.Cell().Element(DataCellStyle).Text(item.Date.ToString("yyyy-MM-dd"));
                    
                    table.Cell().Element(DataCellStyle).Row(row => 
                    {
                        if (barWidth > 0) row.RelativeItem(barWidth).Background(Colors.Blue.Lighten2).Height(8);
                        if (barWidth < 1) row.RelativeItem(1 - barWidth);
                    });
                    
                    table.Cell().Element(DataCellStyle).AlignRight().Text(item.Movements.ToString())
                        .SemiBold().FontColor(Colors.Blue.Darken2);
                }
            });
        });
    }

    /// <summary>
    /// Composes the costs summary widget content.
    /// </summary>
    /// <param name="container">The container to compose the widget in.</param>
    /// <param name="period">The report period.</param>
    private void ComposeCostsSummaryWidget(IContainer container, ReportPeriod period)
    {
        var totalCost = context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedDate >= period.StartDate && o.CreatedDate <= period.EndDate)
            .Join(context.Supplies,
                order => order.ProductId,
                supply => supply.Id,
                (order, supply) => new { order.Quantity, supply.Price }
            )
            .Sum(x => (double)x.Quantity * (double)x.Price);

        container.Background(Colors.Green.Lighten5).Padding(15).Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(localizer["TotalCost"].Value).FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                    col.Item().Text(localizer["TotalDesc"].Value).FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(180).AlignRight().Column(col => 
                {
                    col.Item().Text($"${totalCost:N2}").FontSize(24).Bold().FontColor(Colors.Green.Darken3);
                });
            });

            column.Item().LineHorizontal(1).LineColor(Colors.Green.Lighten3);

            column.Item().Row(row =>
            {
                row.RelativeItem()
                    .DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken2))
                    .Text(text =>
                    {
                        text.Span($"{localizer["ReportRange"]}: ").SemiBold();
                        text.Span($"{period.StartDate:MMM dd, yyyy} - {period.EndDate:MMM dd, yyyy}");
                    });
                
                row.ConstantItem(100).AlignRight().Text($"{period.DaysDuration} {localizer["Days"]} {localizer["Captured"]}")
                    .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
            });
        });
    }

    /// <summary>
    /// Defines the style for table header cells.
    /// </summary>
    /// <param name="c">The container to apply styles to.</param>
    /// <returns>The styled container.</returns>
    private static IContainer HeaderCellStyle(IContainer c) => c
        .Background(Colors.Grey.Lighten3).Padding(6)
        .DefaultTextStyle(x => x.FontSize(9).SemiBold().FontColor(Colors.Grey.Darken3));

    /// <summary>
    /// Defines the style for table data cells.
    /// </summary>
    /// <param name="c">The container to apply styles to.</param>
    /// <returns>The styled container.</returns>
    private static IContainer DataCellStyle(IContainer c) => c
        .BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6);
}