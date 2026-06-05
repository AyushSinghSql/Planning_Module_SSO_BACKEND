using PlanningAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class VarianceReport : IDocument
{
    private readonly List<VarianceComparison> _varianceData;
    private readonly Dictionary<string, string> _aiSummaries;
    private readonly int _versionA;
    private readonly int _versionB;

    public VarianceReport(List<VarianceComparison> varianceData, Dictionary<string, string> aiSummaries, int versionA, int versionB)
    {
        _varianceData = varianceData;
        _aiSummaries = aiSummaries;
        _versionA = versionA;
        _versionB = versionB;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9));

            // Header
            page.Header().Text($"Revenue Variance Report")
                .SemiBold().FontSize(16).AlignCenter().FontColor(Colors.Blue.Darken2);

            page.Content().Column(col =>
            {
                var byPlType = _varianceData
                    .OrderBy(v => v.Year)
                    .ThenBy(v => v.Month)
                    .GroupBy(v => v.PlType);

                foreach (var group in byPlType)
                {
                    // Shaded plan type section
                    col.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(subCol =>
                    {
                        subCol.Item().AlignCenter().Text($"Budget Version - {_versionA} vs EAC Version - {_versionB}")
                            .Bold().FontSize(12).FontColor(Colors.Blue.Darken2);

                        // Table
                        subCol.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100); // Project
                                columns.ConstantColumn(60);  // Month
                                //columns.RelativeColumn();    // Forecasted A
                                //columns.RelativeColumn();    // Forecasted B
                                //columns.RelativeColumn();    // Forecasted Diff
                                //columns.RelativeColumn();    // Actual A
                                //columns.RelativeColumn();    // Actual B
                                //columns.RelativeColumn();    // Actual Diff
                                columns.RelativeColumn();    // Revenue A
                                columns.RelativeColumn();    // Revenue B
                                columns.RelativeColumn();    // Revenue Diff
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Project").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Month").Bold();

                                //string[] categories = { "Forecast", "Actual", "Revenue" };
                                string[] categories = { "Revenue" };

                                foreach (var cat in categories)
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text($"BUD V-{_versionA}").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text($"EAC V-{_versionB}").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text($"Variance").Bold();
                                }
                            });

                            // Table rows
                            bool alternate = false;
                            foreach (var v in group)
                            {
                                var rowColor = alternate ? Colors.White : Colors.Grey.Lighten5;
                                alternate = !alternate;

                                table.Cell().Background(rowColor).Padding(5).Text(v.ProjId);
                                table.Cell().Background(rowColor).Padding(5).Text($"{v.Month}/{v.Year}");

                                //// Forecast
                                //table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.ForecastedCostA));
                                //table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.ForecastedCostB));
                                //table.Cell().Background(rowColor).Padding(5)
                                //    .Text(WithDiffIcon(v.ForecastedCostDiff))
                                //    .FontColor(v.ForecastedCostDiff >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);

                                //// Actual
                                //table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.ActualCostA));
                                //table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.ActualCostB));
                                //table.Cell().Background(rowColor).Padding(5)
                                //    .Text(WithDiffIcon(v.ActualCostDiff))
                                //    .FontColor(v.ActualCostDiff >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);

                                // Revenue
                                table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.RevenueA));
                                table.Cell().Background(rowColor).Padding(5).Text(ToUsd(v.RevenueB));
                                table.Cell().Background(rowColor).Padding(5)
                                    .Text(WithDiffIcon(v.RevenueDiff))
                                    .FontColor(v.RevenueDiff >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            }
                        });

                        // AI Summary box
                        if (_aiSummaries.TryGetValue(group.Key, out var aiText))
                        {
                            subCol.Item().PaddingTop(5).Background(Colors.Blue.Lighten5).Padding(5).Column(aiCol =>
                            {
                                aiCol.Item().Text("Summary:").Bold().FontColor(Colors.Blue.Darken2);
                                aiCol.Item().Text(aiText).Italic();
                            });
                        }
                    });
                }
            });

            // Footer with page numbers
            // Replace this line:
            // page.Footer().AlignCenter().Text(txt => txt.CurrentPageNumber().Text(" / ").TotalPages());

            // With the following:
            page.Footer().AlignCenter().Text(txt =>
            {
                txt.CurrentPageNumber();
                txt.Span(" / ");
                txt.TotalPages();
            });
        });
    }

    private string ToUsd(decimal? value) =>
        string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", value);

    private string WithDiffIcon(decimal? value)
    {
        if (!value.HasValue) return "-";
        return value.Value >= 0 ? $"▲ {ToUsd(value)}" : $"▼ {ToUsd(value)}";
    }

    public byte[] GeneratePdf()
    {
        using var ms = new System.IO.MemoryStream();
        this.GeneratePdf(ms);
        return ms.ToArray();
    }
}
