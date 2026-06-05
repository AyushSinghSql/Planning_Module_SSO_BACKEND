using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using WebApi.DTO;

public class RevenueAnalysisReport : IDocument
{
    private readonly PlanForecastSummary _data;
    private readonly CultureInfo _usdCulture = new CultureInfo("en-US");

    public RevenueAnalysisReport(PlanForecastSummary data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

            // 🏷️ Header
            page.Header().Row(row =>
            {
                row.RelativeItem().Text("Revenue Summary & Analysis")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                row.ConstantItem(80).AlignRight().Column(c =>
                {
                    //c.Item().Text($"Version: {_data.Version}").FontSize(9);
                    c.Item().Text(DateTime.Now.ToString("dd MMM yyyy")).FontSize(9);
                });
            });

            // 📘 Main Content
            page.Content().PaddingVertical(15).Column(col =>
            {
                AddProjectOverview(col);
                AddFinancialOverview(col);
                AddMonthlySummary(col);
                col.Item().PageBreak(); // 🔹 Page break before employee summary
                AddEmployeeSummary(col);
                AddInsightsSection(col);
            });

            // 🦶 Footer
            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Generated on ");
                x.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).SemiBold();
            });
        });
    }

    // ---------------------- 📋 Project Info ----------------------
    private void AddProjectOverview(ColumnDescriptor col)
    {
        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(10)
            .Column(section =>
            {
                section.Item().Text("📋 Project Information")
                    .SemiBold().FontSize(13).FontColor(Colors.Blue.Medium);

                section.Item().PaddingTop(5)
                    .Row(row =>
                    {
                        void AddBadge(string label, string value)
                        {
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                // Use RelativeColumn for each badge
                                row.RelativeColumn().PaddingRight(5)
                                   //.Border(1).BorderColor(Colors.Blue.Lighten2)
                                   .Background(Colors.Blue.Lighten5)
                                   .PaddingHorizontal(5).PaddingVertical(2)
                                   .Text(t =>
                                   {
                                       t.Span($"{label}: "); // label bold & colored
                                       t.Span(value);                     // value normal
                                   });
                            }
                        }

                        AddBadge("Project ID", _data.Proj_Id);
                        AddBadge("Plan Type", _data.Type);
                        AddBadge("Version", _data.Version.ToString());
                    });
            });
    }
    // ---------------------- 💰 Financial Overview ----------------------



private void AddFinancialOverview(ColumnDescriptor col)
{
    col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
        .Background(Colors.White)
        .Padding(10)
        .Column(section =>
        {
            section.Item().Text("💰 Financial Overview")
                .SemiBold().FontSize(13).FontColor(Colors.Green.Darken2);

            section.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                });

                // Calculate profit/loss
                var profit = _data.Revenue - _data.TotalCost;

                // Set colors based on profit/loss
                var revenueColor = profit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                var costColor = profit >= 0 ? Colors.Black : Colors.Red.Darken2;

                AddSummaryRow(table, "Total Revenue", _data.Revenue, revenueColor);
                AddSummaryRow(table, "Total Cost", _data.TotalCost, costColor);
                AddSummaryRow(table, "Total Burden Cost", _data.TotalBurdenCost);
                AddSummaryRow(table, "Fringe", _data.TotalFringe);
                AddSummaryRow(table, "Overhead", _data.TotalOverhead);
                AddSummaryRow(table, "G&A", _data.TotalGna);
                AddSummaryRow(table, "Fees", _data.Fees);
            });
        });
}

// Updated AddSummaryRow to format values in USD
private void AddSummaryRow(TableDescriptor table, string label, decimal value, string fontColor = null)
{
    table.Cell().Text(label);
    var cell = table.Cell().Text(value.ToString("C", _usdCulture)); // USD format
    if (!string.IsNullOrEmpty(fontColor))
        cell.FontColor(fontColor);
}

// ---------------------- 📅 Monthly Summary ----------------------

private void AddMonthlySummary(ColumnDescriptor col)
    {
        if (_data.MonthlyRevenueSummary == null || !_data.MonthlyRevenueSummary.Any())
            return;

        col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.White)
            .Padding(10)
            .Column(section =>
            {
                section.Item().Text("📅 Monthly Revenue Summary")
                    .SemiBold().FontSize(13).FontColor(Colors.Purple.Medium);

                section.Item().PaddingTop(5).Table(table =>
                {
                    // Relative columns: fills all available space proportionally
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);  // Month
                        c.RelativeColumn(2);  // Revenue
                        c.RelativeColumn(2);  // Cost
                        c.RelativeColumn(3);  // Other Direct Cost (slightly wider)
                    });

                    // Header row
                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Month").SemiBold().AlignCenter();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Revenue").SemiBold().AlignRight();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Cost").SemiBold().AlignRight();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Other Direct Cost").SemiBold().AlignRight();
                    });

                    // Data rows
                    foreach (var m in _data.MonthlyRevenueSummary)
                    {
                        // Skip rows where all values are 0
                        if (m.Revenue == 0 && m.Cost == 0 && m.OtherDifrectCost == 0)
                            continue;

                        bool loss = m.Revenue < m.Cost;
                        var bgColor = loss ? Colors.Red.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).Padding(3)
                            .Text($"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Month)} {m.Year}")
                            .AlignCenter();
                        table.Cell().Background(bgColor).Padding(3)
                            .Text(string.Format(_usdCulture, "{0:C}", m.Revenue))
                            .AlignRight();
                        table.Cell().Background(bgColor).Padding(3)
                            .Text(string.Format(_usdCulture, "{0:C}", m.Cost))
                            .AlignRight();
                        table.Cell().Background(bgColor).Padding(3)
                            .Text(string.Format(_usdCulture, "{0:C}", m.OtherDifrectCost))
                            .AlignRight();
                    }
                });
            });
    }

    // ---------------------- 👷 Employee Summary ----------------------

    private void AddEmployeeSummary(ColumnDescriptor col)
    {
        if (_data.EmployeeForecastSummary == null || !_data.EmployeeForecastSummary.Any())
            return;

        col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.White)
            .Padding(10)
            .Column(section =>
            {
                section.Item().Text("👷 Employee Forecast Summary")
                    .SemiBold().FontSize(13).FontColor(Colors.Orange.Medium);

                foreach (var emp in _data.EmployeeForecastSummary
                    .OrderByDescending(e => e.Revenue)
                    .Take(6))
                {
                    section.Item().PaddingVertical(4)
                        .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3)
                        .Column(c =>
                        {
                            c.Item().Text($"{emp.Name} ({emp.EmplId})")
                                .SemiBold().FontSize(11).FontColor(Colors.Black);
                            c.Item().Text($"PLC: {emp.PlcCode} | Hours: {emp.TotalForecastedHours} | Revenue: {string.Format(_usdCulture, "{0:C}", emp.Revenue)}")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                }
            });
    }

    // ---------------------- 📈 Insights ----------------------

    private void AddInsightsSection(ColumnDescriptor col)
    {
        var insights = GenerateInsights();
        if (!insights.Any()) return;

        col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(10)
            .Column(section =>
            {
                section.Item().Text("📈 Key Insights & Highlights")
                    .SemiBold().FontSize(13).FontColor(Colors.Blue.Darken2);

                foreach (var insight in insights)
                {
                    section.Item().Text("• " + insight)
                        .FontSize(10.5f).FontColor(Colors.Grey.Darken3);
                }
            });
    }

    // ---------------------- ⚙️ Helpers ----------------------

    private List<string> GenerateInsights()
    {
        var insights = new List<string>();

        if (_data.Revenue != 0 && _data.TotalCost != 0)
        {
            var profit = _data.Revenue - _data.TotalCost;
            var margin = _data.TotalCost != 0 ? profit / _data.TotalCost * 100 : 0;
            insights.Add($"Net Profit: {string.Format(_usdCulture, "{0:C}", profit)} ({margin:F1}% margin).");
        }

        if (_data.MonthlyRevenueSummary?.Any() == true)
        {
            var bestMonth = _data.MonthlyRevenueSummary.OrderByDescending(m => m.Revenue).First();
            var worstMonth = _data.MonthlyRevenueSummary.OrderBy(m => m.Revenue).First();
            insights.Add($"Best Month: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bestMonth.Month)} {bestMonth.Year} with {string.Format(_usdCulture, "{0:C}", bestMonth.Revenue)} revenue.");
            insights.Add($"Lowest Month: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(worstMonth.Month)} {worstMonth.Year} with {string.Format(_usdCulture, "{0:C}", worstMonth.Revenue)} revenue.");
        }

        if (_data.EmployeeForecastSummary?.Any() == true)
        {
            var topEmp = _data.EmployeeForecastSummary.OrderByDescending(e => e.Revenue).First();
            insights.Add($"Top Performer: {topEmp.Name} ({topEmp.EmplId}) contributed {string.Format(_usdCulture, "{0:C}", topEmp.Revenue)} in revenue.");
        }

        return insights;
    }
    private void AddSummaryRow(TableDescriptor table, string label, decimal? value)
    {
        table.Cell().Element(CellLabel).Text(label).FontColor(Colors.Grey.Darken2);
        table.Cell().Element(CellValue)
            .Text(value.HasValue ? string.Format(_usdCulture, "{0:C}", value.Value) : "-")
            .FontColor(Colors.Black);
    }

    private static IContainer CellLabel(IContainer container) =>
        container.PaddingVertical(3).PaddingRight(5).AlignRight();

    private static IContainer CellValue(IContainer container) =>
        container.PaddingVertical(3);
}

