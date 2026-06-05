using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using WebApi.DTO;

namespace PlanningAPI.Models
{
    public class ForecastReport : IDocument
    {
        private readonly PlanForecastSummary _model;
        private readonly string _aiInsight;

        public ForecastReport(PlanForecastSummary model, string aiInsight)
        {
            _model = model;
            _aiInsight = aiInsight;
        }

        public class ForecastShiftByProjectRequest
        {
            public string ProjectId { get; set; } = null!;
            public string PlanType { get; set; } = null!;
            public int? Version { get; set; } = null!;

            public int SourceYear { get; set; }
            public int TargetYear { get; set; }

            public int SourcePeriod { get; set; }
            public int TargetPeriod { get; set; }

            public decimal Percentage { get; set; } = 0;

            public string PeriodType { get; set; } = "Monthly";
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text($"Forecast Report - {_model.Proj_Id}")
                    .FontSize(20).SemiBold().AlignCenter();

                page.Content().Column(col =>
                {
                    // Existing Forecast Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Employee");
                            header.Cell().Text("Hours");
                            header.Cell().Text("Cost");
                        });

                        foreach (var emp in _model.EmployeeForecastSummary)
                        {
                            table.Cell().Text(emp.EmplId);
                            table.Cell().Text($"{emp.TotalForecastedHours}");
                            table.Cell().Text($"{emp.TotalForecastedCost:C}");
                        }
                    });

                    col.Spacing(15);

                    // 🔹 AI-Generated Insights Section
                    if (!string.IsNullOrWhiteSpace(_aiInsight))
                    {
                        col.Item().Border(1).Padding(10).Background("#F5F5F5").Column(ai =>
                        {
                            ai.Item().Text("AI Insights")
                                .FontSize(14).Bold().FontColor("#2E86C1");

                            ai.Item().Text(_aiInsight)
                                .FontSize(11)
                                .FontColor("#333333");
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ").FontSize(9);
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontSize(9).Italic();
                    });
            });
        }

        //public class FinancialNode
        //{
        //    public string Id { get; set; }
        //    public string Name { get; set; }
        //    public string Type { get; set; }

        //    // For group / account / project levels
        //    public Dictionary<string, decimal> MonthlyTotals { get; set; }

        //    // Recursive children
        //    public List<FinancialNode> Children { get; set; }

        //    // Only for employee/vendor level
        //    public Dictionary<int, Dictionary<string, decimal>> Data { get; set; }
        //}

        //public enum NodeType
        //{
        //    Group,
        //    Sub,
        //    Account,
        //    Project,
        //    Employee
        //}
    }
    public class ProjectFinancialSummary
    {
        public string ProjId { get; set; }

        public decimal Budget { get; set; }

        public decimal Forecast { get; set; }

        public decimal Actuals { get; set; }

        public decimal ETC { get; set; }

        public decimal PriorYearActuals { get; set; }

        public decimal VarianceVsBudget { get; set; }

        public decimal VarianceVsPriorYear { get; set; }

        public decimal PercentVsBudget { get; set; }

        public decimal PercentVsPriorYear { get; set; }
    }
    public class FinancialCardResponse
    {
        public string Title { get; set; }

        public decimal MainValue { get; set; }

        public ComparisonData LeftComparison { get; set; }

        public ComparisonData RightComparison { get; set; }
    }

    public class ComparisonData
    {
        public string Label { get; set; }

        public decimal Value { get; set; }

        public decimal Percentage { get; set; }

        public string Trend { get; set; }
    }


    [Table("trnd_bs_rpt")]
    public class TrndBsRpt
    {
        [Column("acct_id")]
        public string? AcctId { get; set; }

        [Column("acct_name")]
        public string? AcctName { get; set; }

        [Column("fy_cd")]
        public int? FyCd { get; set; }

        [Column("jan")]
        public string? Jan { get; set; }

        [Column("feb")]
        public string? Feb { get; set; }

        [Column("mar")]
        public string? Mar { get; set; }

        [Column("apr")]
        public string? Apr { get; set; }

        [Column("may")]
        public string? May { get; set; }

        [Column("jun")]
        public string? Jun { get; set; }

        [Column("jul")]
        public string? Jul { get; set; }

        [Column("aug")]
        public string? Aug { get; set; }

        [Column("sep")]
        public string? Sep { get; set; }

        [Column("oct")]
        public string? Oct { get; set; }

        [Column("nov")]
        public string? Nov { get; set; }

        [Column("dec")]
        public string? Dec { get; set; }

        [Column("time_stamp")]
        public DateTimeOffset? TimeStamp { get; set; }
    }

    [Table("unanet_vw_psr_combined_data")]
    public class ForecastReportData
    {
        [Column("source_table")]
        public string? SourceTable { get; set; }

        [Column("proj_id")]
        public string? ProjId { get; set; }

        [Column("acct_id")]
        public string? AcctId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("fy_cd")]
        public string? FyCd { get; set; }

        [Column("pd_no")]
        public int? PdNo { get; set; }

        [Column("sub_pd_no")]
        public int? SubPdNo { get; set; }

        [Column("pool_no")]
        public string? PoolNo { get; set; }

        [Column("sub_tot_type_no")]
        public int? SubTotTypeNo { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("hours")]
        public decimal? Hours { get; set; }

        [Column("lab_hs_key")]
        public int? LabHsKey { get; set; }

        [Column("empl_id")]
        public string? EmplId { get; set; }

        [Column("vend_id")]
        public string? VendId { get; set; }

        [Column("vend_empl_id")]
        public string? VendEmplId { get; set; }

        [Column("s_id_type")]
        public string? SIdType { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("s_jnl_cd")]
        public string? SJnlCd { get; set; }

        [Column("rate_type")] public string? RateType { get; set; }
    }
}
