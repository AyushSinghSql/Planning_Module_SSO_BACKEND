using PlanningAPI.Models;

namespace PlanningAPI.Helpers
{
    public class MonthlyCostRevenue
    {
        public int Id { get; set; }

        public int ForecastId { get; set; }      // Foreign key to pl_forecast
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Empl_id { get; set; }
        public decimal Cost { get; set; } = 0.00M;
        public decimal Burden { get; set; } = 0.00M;

        // Note: EF Core does not support computed/generated columns in model directly
        public decimal BurdenedCost { get; private set; }

        public decimal Revenue { get; set; } = 0.00M;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property (optional, if you want to access forecast)
        public PlForecast Forecast { get; set; }
    }
    public class MonthlyData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal LaborCost { get; set; } = 0.00M;
        public decimal NonLaborCost { get; set; } = 0.00M;
        public decimal Fringe { get; set; } = 0.00M;
        public decimal Gna { get; set; } = 0.00M;
        public decimal Mnh { get; set; } = 0.00M;
        public decimal Hr { get; set; } = 0.00M;
        public decimal Overhead { get; set; } = 0.00M;
        public decimal Revenue { get; set; } = 0.00M;
        public decimal Hours { get; set; } = 0.00M;

    }

    public class MonthlyDataV1
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? OrgId { get; set; }
        public string? AcctId { get; set; }
        public decimal LaborCost { get; set; } = 0.00M;
        public decimal NonLaborCost { get; set; } = 0.00M;
        public decimal Fringe { get; set; } = 0.00M;
        public decimal Gna { get; set; } = 0.00M;
        public decimal Mnh { get; set; } = 0.00M;
        public decimal Hr { get; set; } = 0.00M;
        public decimal Overhead { get; set; } = 0.00M;
        public decimal Revenue { get; set; } = 0.00M;
        public decimal Hours { get; set; } = 0.00M;
        public string? FringeName { get; set; }
        public string? OverheadName { get; set; }
        public string? GnaName { get; set; }
        public string? MaterialsName { get; set; }
        public string? HrName { get; set; }

    }

    public class MonthlyDataV2
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal LaborCost { get; set; } = 0.00M;
        public decimal NonLaborCost { get; set; } = 0.00M;
        public decimal Hours { get; set; } = 0.00M;
        public List<Indirect> IndirectCost { get; set; } = new List<Indirect>();
    }
    public class IndirectRates
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; } = 0.00M;

    }
    public class Indirect
    {
        public string? Name { get; set; }
        public decimal Value { get; set; } = 0.00M;

    }
}
