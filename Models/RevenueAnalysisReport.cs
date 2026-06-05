using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

public class RevenueChartPdf : IDocument
{
    private readonly List<MonthlyRevenueSummary> months;

    public RevenueChartPdf(List<MonthlyRevenueSummary> months)
    {
        this.months = months;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10));

            // Header
            page.Header()
                .Text("Revenue, Cost & Profit Summary")
                .Bold().FontSize(16).AlignCenter();

            page.Content().Column(col =>
            {
                // Summary table
                col.Item().PaddingBottom(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Total Revenue").Bold();
                        header.Cell().Text("Total Cost").Bold();
                        header.Cell().Text("Total Profit").Bold();
                    });

                    decimal totalRevenue = months.Sum(m => m.Revenue);
                    decimal totalCost = months.Sum(m => m.Cost);
                    decimal totalProfit = totalRevenue - totalCost;

                    table.Cell().Text($"{totalRevenue:C}");
                    table.Cell().Text($"{totalCost:C}");
                    table.Cell().Text($"{totalProfit:C}");
                });

                // Legend
                col.Item().PaddingBottom(10).Row(row =>
                {
                    row.ConstantItem(100).Stack(stack =>
                    {
                        stack.Item()
                             .Height(10)
                             .Width(10)
                             .Background(Colors.Green.Medium);
                        stack.Item().PaddingLeft(4)
                             .Text("Revenue")
                             .FontSize(9)
                             ;
                    });

                    row.ConstantItem(100).Stack(stack =>
                    {
                        stack.Item()
                             .Height(10)
                             .Width(10)
                             .Background(Colors.Red.Medium);
                        stack.Item().PaddingLeft(4)
                             .Text("Cost")
                             .FontSize(9)
                             ;
                    });

                    row.ConstantItem(100).Stack(stack =>
                    {
                        stack.Item()
                             .Height(2)
                             .Width(20)
                             .Background(Colors.Blue.Medium);
                        stack.Item().PaddingLeft(4)
                             .Text("Profit")
                             .FontSize(9)
                             ;
                    });
                });

                // Chart (bars + profit line using text labels)
                col.Item().Height(150).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in months)
                            columns.RelativeColumn();
                    });

                    // Revenue bars
                    table.Cell().Row(row =>
                    {
                        decimal maxVal = Math.Max(months.Max(m => Math.Max(m.Revenue, m.Cost)),
                                                  months.Max(m => m.Revenue - m.Cost));
                        if (maxVal == 0) maxVal = 1;

                        foreach (var m in months)
                        {
                            row.ConstantItem(30).Column(bar =>
                            {
                                var revHeight = (int)(m.Revenue / maxVal * 100);
                                var costHeight = (int)(m.Cost / maxVal * 100);

                                bar.Item()
                                   .Height(revHeight)
                                   .Background(Colors.Green.Medium);

                                bar.Item()
                                   .Height(costHeight)
                                   .Background(Colors.Red.Medium);

                                bar.Item()
                                   .Text($"{m.Revenue - m.Cost:C}")
                                   .FontSize(7)
                                   .AlignCenter();
                            });
                        }
                    });

                    // Month labels
                    table.Cell().Row(row =>
                    {
                        foreach (var m in months)
                        {
                            row.ConstantItem(30)
                               .Text(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Month))
                               .FontSize(8)
                               .AlignCenter();
                        }
                    });
                });
            });
        });
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public byte[] GeneratePdf()
    {
        using var ms = new MemoryStream();
        this.GeneratePdf(ms);
        return ms.ToArray();
    }
}

// Model
public class MonthlyRevenueSummary
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
}
