using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models;


public partial class NewBusinessForecast
{
    public decimal? Forecastedamt { get; set; }

    public int Forecastid { get; set; }

    public int PlId { get; set; }

    public string? EmplId { get; set; } = null!;
    public int? DctId { get; set; } = null!;

    public int Month { get; set; }

    public int Year { get; set; }
    [NotMapped]
    public decimal TotalBurdenCost { get; set; }
    [NotMapped]
    public decimal Burden { get; set; }
    [NotMapped]
    public decimal CCFFRevenue { get; set; }
    [NotMapped]
    public decimal TNMRevenue { get; set; }
    [NotMapped]
    public decimal Cost { get; set; }
    [NotMapped]
    public decimal Fringe { get; set; }
    [NotMapped]
    public decimal Overhead { get; set; }
    [NotMapped]
    public decimal Gna { get; set; }

    public decimal Forecastedhours { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    [NotMapped]
    public string? DisplayText { get; set; }

    // ✅ Navigation properties: make nullable so validation doesn't require them
    [JsonIgnore]
    public virtual PlEmployee? Empl { get; set; }

    [JsonIgnore]
    public virtual PlProjectPlan? Pl { get; set; }

    [JsonIgnore]
    public virtual PlProject? Proj { get; set; }

    [JsonIgnore]
    public virtual PlDct? DirectCost { get; set; }

    public static PlForecast CloneWithoutId(PlForecast source)
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
            DisplayText = source.DisplayText
        };
    }
}