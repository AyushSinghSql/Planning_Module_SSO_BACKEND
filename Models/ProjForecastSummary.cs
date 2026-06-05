using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("proj_forecast_summary", Schema = "public")]
    public class ProjForecastSummary
    {
        [Column("proj_id")] public string ProjId { get; set; }
        [Column("pl_type")] public string PlType { get; set; }
        [Column("version")] public int Version { get; set; }
        [Column("year")] public int Year { get; set; }
        [Column("month")] public int Month { get; set; }
        [Column("monthly_cost")] public decimal? MonthlyCost { get; set; }
        [Column("ytd_cost")] public decimal? YtdCost { get; set; }
        [Column("itd_cost")] public decimal? ItdCost { get; set; }
        [Column("forecasted_monthly_cost")] public decimal? ForecastedMonthlyCost { get; set; }
        [Column("forecasted_ytd_cost")] public decimal? ForecastedYtdCost { get; set; }
        [Column("forecasted_itd_cost")] public decimal? ForecastedItdCost { get; set; }
        [Column("monthly_revenue")] public decimal? MonthlyRevenue { get; set; }
        [Column("ytd_revenue")] public decimal? YtdRevenue { get; set; }
        [Column("itd_revenue")] public decimal? ItdRevenue { get; set; }
        [Column("monthly_burden")] public decimal? MonthlyBurden { get; set; }
        [Column("ytd_burden")] public decimal? YtdBurden { get; set; }
        [Column("itd_burden")] public decimal? ItdBurden { get; set; }
        [Column("monthly_forecasted_hours")] public decimal? MonthlyForecastedHours { get; set; }
        [Column("ytd_forecasted_hours")] public decimal? YtdForecastedHours { get; set; }
        [Column("itd_forecasted_hours")] public decimal? ItdForecastedHours { get; set; }
        [Column("monthly_forecasted_amt")] public decimal? MonthlyForecastedAmt { get; set; }
        [Column("ytd_forecasted_amt")] public decimal? YtdForecastedAmt { get; set; }
        [Column("itd_forecasted_amt")] public decimal? ItdForecastedAmt { get; set; }
        [Column("monthly_actual_hours")] public decimal? MonthlyActualHours { get; set; }
        [Column("ytd_actual_hours")] public decimal? YtdActualHours { get; set; }
        [Column("itd_actual_hours")] public decimal? ItdActualHours { get; set; }
        [Column("monthly_actual_amt")] public decimal? MonthlyActualAmt { get; set; }
        [Column("ytd_actual_amt")] public decimal? YtdActualAmt { get; set; }
        [Column("itd_actual_amt")] public decimal? ItdActualAmt { get; set; }
        [Column("monthly_target_revenue")] public decimal? MonthlyTargetRevenue { get; set; }
        [Column("ytd_target_revenue")] public decimal? YtdTargetRevenue { get; set; }
        [Column("itd_target_revenue")] public decimal? ItdTargetRevenue { get; set; }
        [Column("monthly_target_burden")] public decimal? MonthlyTargetBurden { get; set; }
        [Column("ytd_target_burden")] public decimal? YtdTargetBurden { get; set; }
        [Column("itd_target_burden")] public decimal? ItdTargetBurden { get; set; }
    }

}
