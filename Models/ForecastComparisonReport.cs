using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanningAPI.Models
{
    public class ForecastComparison
    {
        public string ProjId { get; set; }
        public string AcctId { get; set; }
        public string AccountName { get; set; }

        public decimal BudRevenue { get; set; }
        public decimal EacRevenue { get; set; }
        public decimal RevenueVariance => EacRevenue - BudRevenue;

        public decimal BudCost { get; set; }
        public decimal EacCost { get; set; }
        public decimal CostVariance => EacCost - BudCost;

        public decimal BudBurden { get; set; }
        public decimal EacBurden { get; set; }
        public decimal BurdenVariance => EacBurden - BudBurden;
    }

    public class ForecastComparisonReport : IDocument
    {
        private readonly List<ForecastComparison> _data;

        public ForecastComparisonReport(List<ForecastComparison> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "BUD vs EAC Forecast Comparison",
            Author = "Your Company"
        };

        public void Compose(IDocumentContainer container)
        {
            // Find max variances for bar scaling
            var maxRevenueVariance = _data.Max(x => Math.Abs(x.RevenueVariance));
            var maxCostVariance = _data.Max(x => Math.Abs(x.CostVariance));
            var maxBurdenVariance = _data.Max(x => Math.Abs(x.BurdenVariance));

            // Totals
            var totalBudRevenue = _data.Sum(x => x.BudRevenue);
            var totalEacRevenue = _data.Sum(x => x.EacRevenue);
            var totalRevenueVariance = totalEacRevenue - totalBudRevenue;

            var totalBudCost = _data.Sum(x => x.BudCost);
            var totalEacCost = _data.Sum(x => x.EacCost);
            var totalCostVariance = totalEacCost - totalBudCost;

            var totalBudBurden = _data.Sum(x => x.BudBurden);
            var totalEacBurden = _data.Sum(x => x.EacBurden);
            var totalBurdenVariance = totalEacBurden - totalBudBurden;

            container.Page(page =>
            {
                page.Margin(20);

                page.Header()
                    .Text("Forecast Comparison: BUD vs EAC")
                    .FontSize(18)
                    .SemiBold();

                page.Content()
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Account
                            columns.RelativeColumn(1); // BUD Revenue
                            columns.RelativeColumn(1); // EAC Revenue
                            columns.RelativeColumn(1); // Revenue Variance
                            columns.RelativeColumn(1); // BUD Cost
                            columns.RelativeColumn(1); // EAC Cost
                            columns.RelativeColumn(1); // Cost Variance
                            columns.RelativeColumn(1); // BUD Burden
                            columns.RelativeColumn(1); // EAC Burden
                            columns.RelativeColumn(1); // Burden Variance
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Text("Account").SemiBold();
                            header.Cell().Text("BUD Revenue").SemiBold();
                            header.Cell().Text("EAC Revenue").SemiBold();
                            header.Cell().Text("Variance").SemiBold();
                            header.Cell().Text("BUD Cost").SemiBold();
                            header.Cell().Text("EAC Cost").SemiBold();
                            header.Cell().Text("Variance").SemiBold();
                            header.Cell().Text("BUD Burden").SemiBold();
                            header.Cell().Text("EAC Burden").SemiBold();
                            header.Cell().Text("Variance").SemiBold();
                        });

                        // Data rows with zebra stripes
                        var rowIndex = 0;
                        foreach (var row in _data)
                        {
                            var bgColor = rowIndex % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                            table.Cell().Background(bgColor).Text(row.AccountName);

                            table.Cell().Background(bgColor).Text(row.BudRevenue.ToString("C"));
                            table.Cell().Background(bgColor).Text(row.EacRevenue.ToString("C"));

                            table.Cell().Background(bgColor).Padding(2).Element(cell =>
                            {
                                var barWidth = maxRevenueVariance == 0 ? 0 : Math.Abs(row.RevenueVariance) / maxRevenueVariance * 100;
                                var barColor = row.RevenueVariance >= 0 ? Colors.Green.Medium : Colors.Red.Medium;

                                cell.Stack(stack =>
                                {
                                    //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                    //stack.Element().Text(row.RevenueVariance.ToString("C"))
                                    //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                                });
                            });

                            table.Cell().Background(bgColor).Text(row.BudCost.ToString("C"));
                            table.Cell().Background(bgColor).Text(row.EacCost.ToString("C"));

                            table.Cell().Background(bgColor).Padding(2).Element(cell =>
                            {
                                var barWidth = maxCostVariance == 0 ? 0 : Math.Abs(row.CostVariance) / maxCostVariance * 100;
                                var barColor = row.CostVariance >= 0 ? Colors.Green.Medium : Colors.Red.Medium;

                                cell.Stack(stack =>
                                {
                                    //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                    //stack.Element().Text(row.CostVariance.ToString("C"))
                                    //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                                });
                            });

                            table.Cell().Background(bgColor).Text(row.BudBurden.ToString("C"));
                            table.Cell().Background(bgColor).Text(row.EacBurden.ToString("C"));

                            table.Cell().Background(bgColor).Padding(2).Element(cell =>
                            {
                                var barWidth = maxBurdenVariance == 0 ? 0 : Math.Abs(row.BurdenVariance) / maxBurdenVariance * 100;
                                var barColor = row.BurdenVariance >= 0 ? Colors.Green.Medium : Colors.Red.Medium;

                                cell.Stack(stack =>
                                {
                                    //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                    //stack.Element().Text(row.BurdenVariance.ToString("C"))
                                    //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                                });
                            });

                            rowIndex++;
                        }

                        // Summary row
                        void AddSummaryCell(string text) => table.Cell().Background(Colors.Blue.Lighten4).Text(text).SemiBold();

                        AddSummaryCell("TOTAL");

                        AddSummaryCell(totalBudRevenue.ToString("C"));
                        AddSummaryCell(totalEacRevenue.ToString("C"));
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(2).Element(cell =>
                        {
                            var barWidth = maxRevenueVariance == 0 ? 0 : Math.Abs(totalRevenueVariance) / maxRevenueVariance * 100;
                            var barColor = totalRevenueVariance >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1;
                            cell.Stack(stack =>
                            {
                                //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                //stack.Element().Text(totalRevenueVariance.ToString("C"))
                                //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                            });
                        });

                        AddSummaryCell(totalBudCost.ToString("C"));
                        AddSummaryCell(totalEacCost.ToString("C"));
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(2).Element(cell =>
                        {
                            var barWidth = maxCostVariance == 0 ? 0 : Math.Abs(totalCostVariance) / maxCostVariance * 100;
                            var barColor = totalCostVariance >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1;
                            cell.Stack(stack =>
                            {
                                //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                //stack.Element().Text(totalCostVariance.ToString("C"))
                                //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                            });
                        });

                        AddSummaryCell(totalBudBurden.ToString("C"));
                        AddSummaryCell(totalEacBurden.ToString("C"));
                        table.Cell().Background(Colors.Blue.Lighten4).Padding(2).Element(cell =>
                        {
                            var barWidth = maxBurdenVariance == 0 ? 0 : Math.Abs(totalBurdenVariance) / maxBurdenVariance * 100;
                            var barColor = totalBurdenVariance >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1;
                            cell.Stack(stack =>
                            {
                                //stack.Element().Width(barWidth).Height(12).Background(barColor);
                                //stack.Element().Text(totalBurdenVariance.ToString("C"))
                                //     .FontSize(10).FontColor(Colors.Black).AlignCenter();
                            });
                        });
                    });
            });
        }
    }

}
