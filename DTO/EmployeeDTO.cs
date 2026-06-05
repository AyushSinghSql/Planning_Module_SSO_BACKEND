using System.ComponentModel.DataAnnotations.Schema;
using PlanningAPI.Models;

namespace WebApi.DTO
{

    public class EmployeeDTOs
    {
        public string? EmpId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? HrRate { get; set; }
        public string? Plc { get; set; }
        public string? OrgId { get; set; }
        public string? OrgName { get; set; }
        public string? AcctId { get; set; }
        public string? AcctName { get; set; }
        public string? LaborGroup { get; set; }
        public DateOnly? EffectiveDate { get; set; } = new DateOnly();

    }

    public class VendorEmployeeDTOs
    {
        public string? EmpId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Plc { get; set; }
        public string? VendId { get; set; }
        public string? OrgId { get; set; }
        public string? OrgName { get; set; }
        public string? AcctId { get; set; }
        public string? AcctName { get; set; }
    }
    public class EmployeeDTO
    {
        public string? EmpId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? HrRate { get; set; }

        public string? ProjectId { get; set; }
        public string? OrgID { get; set; }
        public string? Plc { get; set; }

        public string? AccID { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public DateOnly? EndDate { get; set; }

    }
    public class EmplSchedule
    {
        public string EmpId { get; set; }
        public string PlcCode { get; set; }
        public string? OrgID { get; set; }
        public string? AccID { get; set; }
        public bool isRev { get; set; } = false;


        public List<MonthlySalary> payrollSalary { get; set; }

    }

    public class DirectCostSchedule
    {
        public int DctId { get; set; }
        public string AcctId { get; set; }
        public string OrgID { get; set; }
        public bool isRev { get; set; } = false;

        public List<PlForecast> forecasts { get; set; }

    }
    public class MonthlySalary
    {
        public string EmpId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Salary { get; set; }
        public decimal SalRatio { get; set; }
        public decimal Hours { get; set; }
        public decimal Cost { get; set; }
        public decimal Fringe { get; set; }
        public decimal Overhead { get; set; }
        public decimal Gna { get; set; }
        public decimal Materials { get; set; }
        public decimal Hr { get; set; }
        public decimal Burden { get; set; }
        public decimal TotalBurdenCost { get; set; }
        public decimal CCFFRevenue { get; set; }
        public decimal TNMRevenue { get; set; }
        public decimal Revenue { get; set; }
        public decimal Fees { get; set; } = 0m;



    }
    public class Schedule
    {

        public int Year { get; set; }
        public int MonthNo { get; set; }
        public decimal WorkingHours { get; set; }
        [NotMapped]
        public string Month { get; set; }
        public int WorkingDays { get; set; }

    }

    public class EmplForecastDTO
    {
        public int Pl_ID { get; set; }
        public string Empl_Id { get; set; }
        public List<forecostHours1>? emplHrs { get; set; }
    }
    public class forecostHours1
    {
        public int ForecastId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ForecastedHours { get; set; }
    }
    public class EmplForecast
    {
        public int ForecastId { get; set; }
        public int Pl_ID { get; set; }
        public string Empl_Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ForecastedHours { get; set; }
        public string Proj_Id { get; set; }
    }
    public class MonthlyRevenueSummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal OtherDifrectCost { get; set; }

    }

    public class MonthlySummary
    {
        public string Proj_Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int subTotalType { get; set; }
        public decimal Cost { get; set; }

    }
    public class StatusResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class MonthlyForCalSummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string RateType { get; set; }
        public int subTotalType { get; set; }
        public decimal Cost { get; set; }

    }
    public class MonthlyFeeSummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TargetCost { get; set; }
        public decimal TargetRevenue   { get; set; }
        public decimal ActualCost { get; set; }
        public decimal ActualRevenue { get; set; }
        public decimal ActualFeeOnRevenue { get; set; }
        public decimal TargetFeeOnRevenue { get; set; }
        public decimal ActualFeeOnCost { get; set; }
        public decimal TargetFeeOnCost { get; set; }
        public decimal CalculatedActualFee { get; set; }
        public decimal CalculatedTargetFee { get; set; }
    }

    public class MonthlyActualRevenueSummary
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int subTotalType { get; set; }
        public decimal Revenue { get; set; }
        public decimal ItdRevenue { get; set; }
        public decimal YtdRevenue { get; set; }


    }
    public class EmployeeForecastSummary
    {
        public string OrgID { get; set; }

        public string AccID { get; set; }
        public string EmplId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal TotalForecastedHours { get; set; }
        public decimal PerHourRate { get; set; }
        public decimal TotalForecastedCost { get; set; }
        public decimal Burden { get; set; }
        public decimal TotalBurdonCost { get; set; }
        public decimal Fringe { get; set; }
        public decimal Overhead { get; set; }
        public decimal Gna { get; set; }
        public decimal Materials { get; set; }
        public decimal Hr { get; set; }
        public decimal TNMRevenue { get; set; }
        public decimal CPFFRevenue { get; set; }
        public decimal Revenue { get; set; }
        public decimal Fees { get; set; } = 0m;

        public string? FringeName { get; set; }
        public string? OverheadName { get; set; }
        public string? GnaName { get; set; }
        public string? MaterialsName { get; set; }
        public string? HrName { get; set; }
        public string PlcCode { get; set; }
        public EmplSchedule? emplSchedule { get; set; }
        public DirectCostSchedule? directCostSchedule { get; set; }


    }

    public class PlanForecastSummary
    {
        public List<EmployeeForecastSummary> EmployeeForecastSummary { get; set; }
        public List<EmployeeForecastSummary> DirectCOstForecastSummary { get; set; }
        public List<MonthlyRevenueSummary> MonthlyRevenueSummary { get; set; }
        public decimal TotalCost { get; set; } = 0m;
        public decimal Fees { get; set; } = 0m;
        public decimal TotalGna { get; set; }
        public decimal TotalOverhead { get; set; }
        public decimal TotalFringe { get; set; }
        public decimal TotalMaterials { get; set; }
        public decimal TotalHr { get; set; }
        public decimal TotalBurden { get; set; }
        public decimal TotalBurdenCost { get; set; }
        public decimal TNMRevenue { get; set; }
        public decimal CPFFRevenue { get; set; }
        public decimal Revenue { get; set; }
        public decimal AdjustedRevenue { get; set; }
        public decimal AtRiskAmt { get; set; }
        public decimal FundingValue { get; set; }
        public string RevenueFormula { get; set; }
        public string Proj_Id { get; set; }
        public string Type { get; set; }
        public int Version { get; set; }

    }

    public class MonthlyEmployeeHours
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? EmployeeId { get; set; }
        public decimal ActualHours { get; set; }
        public string AccId { get; set; }

        public string OrgId { get; set; }
        public string Plc { get; set; }

        public string? VendId { get; set; }
        public string? VendEmplId { get; set; }
    }


}
