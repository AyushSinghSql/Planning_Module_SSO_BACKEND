using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebApi.DTO;

namespace PlanningAPI.Models;


public partial class PlForecast
{
    public decimal? Forecastedamt { get; set; }
    public decimal? Actualamt { get; set; }


    public int Forecastid { get; set; }

    public string? ProjId { get; set; }

    public int PlId { get; set; }

    public string? EmplId { get; set; } = null!;
    public int? DctId { get; set; } = null!;

    public int Month { get; set; }

    public int Year { get; set; }
    [NotMapped]
    public decimal TotalBurdenCost { get; set; }
    public decimal Fees { get; set; } = 0m;

    public decimal Burden { get; set; } = decimal.Zero;
    [NotMapped]
    public decimal CCFFRevenue { get; set; }
    [NotMapped]
    public decimal TNMRevenue { get; set; }
    public decimal Revenue { get; set; } = decimal.Zero;
    public decimal Cost { get; set; } = decimal.Zero;

    public decimal? ForecastedCost { get; set; } = decimal.Zero;


    public decimal Fringe { get; set; } = decimal.Zero;
    public decimal Overhead { get; set; } = decimal.Zero;
    public decimal Gna { get; set; } = decimal.Zero;
    public decimal Materials { get; set; } = decimal.Zero;

    public decimal YtdFringe { get; set; } = decimal.Zero;
    public decimal YtdOverhead { get; set; } = decimal.Zero;
    public decimal YtdGna { get; set; } = decimal.Zero;
    public decimal YtdMaterials { get; set; } = decimal.Zero;
    public decimal YtdCost { get; set; } = decimal.Zero;
    public decimal Hr { get; set; } = decimal.Zero;
    public decimal Forecastedhours { get; set; }
    public decimal Actualhours { get; set; }


    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    [NotMapped]
    public string? DisplayText { get; set; }

    public string? AcctId { get; set; }
    public string? OrgId { get; set; }
    public string? Plc { get; set; }
    [NotMapped]
    public decimal? CTDHours { get; set; }

    [NotMapped]
    public decimal? PriorYearHours { get; set; }

    public int? empleId { get; set; }
    public decimal? HrlyRate { get; set; }
    public DateOnly? EffectDt { get; set; } = null;
    //public DateOnly? Esc_Month { get; set; } = null;

    // ✅ Navigation properties: make nullable so validation doesn't require them
    [JsonIgnore]
    public virtual PlEmployee? Empl { get; set; }
    public virtual PlEmployeee? Emple { get; set; }


    [JsonIgnore]
    public virtual PlProjectPlan? Pl { get; set; }

    [JsonIgnore]
    public virtual PlProject? Proj { get; set; }

    [JsonIgnore]
    public virtual PlDct? DirectCost { get; set; }
    //[JsonIgnore]
    //public ProjectPlcRate? PlcRate { get; set; }
    public static PlForecast CloneWithoutIdForEAC(PlForecast source)
    {
        if (source == null) return null;

        return new PlForecast
        {
            Forecastedamt = source.Forecastedamt,
            Forecastedhours = source.Forecastedhours,
            EmplId = source.EmplId,
            Month = source.Month,
            Year = source.Year,
            ProjId = source.ProjId,
            AcctId = source.AcctId,
            OrgId = source.OrgId,
            HrlyRate = source.HrlyRate,
            Plc = source.Plc,
            Emple = source.Emple,
            //EffectDt = source.EffectDt,
            DisplayText = source.DisplayText,
            DirectCost = source.DirectCost,
            Cost = source.Cost,
            Burden = source.Burden,
            Actualamt = source.Forecastedamt,
            Actualhours = source.Forecastedhours,
            TotalBurdenCost = source.TotalBurdenCost,
            Revenue = source.Revenue,
            Fringe = source.Fringe,
            Overhead = source.Overhead,
            Materials = source.Materials,
            Gna = source.Gna,
            Hr = source.Hr,
            YtdGna = source.YtdGna,
            YtdCost = source.YtdCost,
            YtdFringe = source.YtdFringe,
            YtdOverhead = source.YtdOverhead,
            YtdMaterials = source.YtdMaterials,
            ForecastedCost = source.ForecastedCost,
            //Esc_Month = source.Esc_Month
            //Empl = source.Empl
        };
    }
    public static PlForecast CloneWithoutId(PlForecast source)
    {
        if (source == null) return null;

        return new PlForecast
        {
            Forecastedamt = Math.Max(0, source.Forecastedamt.GetValueOrDefault()),
            Forecastedhours = Math.Max(0, source.Forecastedhours),
            Actualamt = Math.Max(0, source.Forecastedamt.GetValueOrDefault()),
            Actualhours = Math.Max(0, source.Forecastedhours),
            EmplId = source.EmplId,
            Month = source.Month,
            Year = source.Year,
            ProjId = source.ProjId,
            AcctId = source.AcctId,
            OrgId = source.OrgId,
            HrlyRate = source.HrlyRate,
            Plc = source.Plc,
            Emple = source.Emple,
            EffectDt = source.EffectDt,
            DisplayText = source.DisplayText,
            DirectCost = source.DirectCost,
            Cost = source.Cost,
            Burden = source.Burden,
            TotalBurdenCost = source.TotalBurdenCost,
            Revenue = source.Revenue,
            Fringe = source.Fringe,
            Overhead = source.Overhead,
            Materials = source.Materials,
            Gna = source.Gna,
            Hr = source.Hr,
            YtdGna = source.YtdGna,
            YtdCost = source.YtdCost,
            YtdFringe = source.YtdFringe,
            YtdOverhead = source.YtdOverhead,
            YtdMaterials = source.YtdMaterials,
            ForecastedCost = source.ForecastedCost,
            //Esc_Month = source.Esc_Month,
            Empl = source.Empl
        };
    }


    public static PlForecast CloneWithoutIdFromEACToBUD(PlForecast source)
    {
        if (source == null) return null;

        return new PlForecast
        {
            Forecastedamt = Math.Max(0, source.Actualamt.GetValueOrDefault()),
            Forecastedhours = Math.Max(0, source.Actualhours),
            Actualamt = Math.Max(0, source.Forecastedamt.GetValueOrDefault()),
            Actualhours = Math.Max(0, source.Forecastedhours),
            EmplId = source.EmplId,
            Month = source.Month,
            Year = source.Year,
            ProjId = source.ProjId,
            AcctId = source.AcctId,
            OrgId = source.OrgId,
            HrlyRate = source.HrlyRate,
            Plc = source.Plc,
            Emple = source.Emple,
            EffectDt = source.EffectDt,
            DisplayText = source.DisplayText,
            DirectCost = source.DirectCost,
            Cost = source.Cost,
            Burden = source.Burden,
            TotalBurdenCost = source.TotalBurdenCost,
            Revenue = source.Revenue,
            Fringe = source.Fringe,
            Overhead = source.Overhead,
            Materials = source.Materials,
            Gna = source.Gna,
            Hr = source.Hr,
            YtdGna = source.YtdGna,
            YtdCost = source.YtdCost,
            YtdFringe = source.YtdFringe,
            YtdOverhead = source.YtdOverhead,
            YtdMaterials = source.YtdMaterials,
            ForecastedCost = source.ForecastedCost,
            //Esc_Month = source.Esc_Month,
            Empl = source.Empl
        };
    }
    public static PlForecast CloneWithoutId1(PlForecast source)
    {
        if (source == null) return null;

        return new PlForecast
        {
            Forecastedamt = source.Forecastedamt,
            Forecastedhours = source.Forecastedhours,
            EmplId = source.EmplId,
            Month = source.Month,
            Year = source.Year,
            ProjId = source.ProjId,
            AcctId = source.AcctId,
            OrgId = source.OrgId,
            HrlyRate = source.HrlyRate,
            Plc = source.Plc,
            EffectDt = source.EffectDt,
            DisplayText = source.DisplayText
        };
    }
}
public class AlternateEmployeeDto
{
    public string EmployeeId { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal AvailableHours { get; set; }
}


public class AlternateEmployeeDetailsDto
{
    public string? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    public string? Plc { get; set; }

    public decimal AssignedHours { get; set; }

    public decimal AvailableHours { get; set; }

    public bool CanAllocate { get; set; }

    public decimal UtilizationPercent { get; set; }

    public decimal? PerHourRate { get; set; }
}
//public partial class PlForecast
//{
//    public int? Forecastedamt { get; set; }

//    public int Forecastid { get; set; }

//    public string? ProjId { get; set; }

//    public int PlId { get; set; }

//    public string EmplId { get; set; } = null!;

//    public int Month { get; set; }

//    public int Year { get; set; }

//    public decimal Forecastedhours { get; set; }

//    public DateTime? Createdat { get; set; }

//    public DateTime? Updatedat { get; set; }

//    [JsonIgnore]
//    public virtual PlEmployee Empl { get; set; } = null!;
//    [JsonIgnore]
//    public virtual PlProjectPlan Pl { get; set; } = null!;
//    [JsonIgnore]
//    public virtual PlProject? Proj { get; set; }
//}
public class PagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public IEnumerable<T> Data { get; set; }
}

//public class FinancialNode
//{
//    public string Id { get; set; }
//    public string Name { get; set; }
//    public string Type { get; set; }

//    public Dictionary<string, decimal> MonthlyTotals { get; set; } = new();

//    public List<FinancialNode> Children { get; set; } = new();

//    public Dictionary<int, Dictionary<string, decimal>> Data { get; set; }
//}


public class FinancialNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }

    public Dictionary<string, decimal> MonthlyTotals { get; set; } = new();
    public Dictionary<string, decimal> RevenueTotals { get; set; } = new();   // ✅ Revenue
    public List<FinancialNode> Children { get; set; } = new();

    // ⚡ Internal lookup (not serialized)
    public Dictionary<string, FinancialNode> Lookup { get; set; } = new();
}
public class ProjectFinancialSummaryDto
{
    public string ProjId { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitPercent { get; set; }
    public decimal Backlog { get; set; }
}
public class ProjectFinancialSummary1Dto
{
    public string ProjId { get; set; } = null!;
    public bool IsRollup { get; set; }

    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitPercent { get; set; }
    public decimal Backlog { get; set; }
    public decimal Funding { get; set; }

}