using System.Text.Json.Serialization;
using PlanningAPI.Models;

namespace WebApi.DTO
{

    public class FundingDetails
    {
        public string Type { get; set; }
        public decimal Funding { get; set; }
        public decimal Budget { get; set; }
        public decimal Balance { get; set; }
        public decimal Percent { get; set; }
        public decimal AtRisk { get; set; }

    }
    public class ProjectDTO
    {
        public string ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string OrgId { get; set; }
        public string? OrgName { get; set; }
        public string? Type { get; set; }
        public string? AccountGroupCode { get; set; }
        public string? RevenueAccount { get; set; }

        public decimal? proj_f_tot_amt { get; set; } = 0m;
        public decimal? proj_f_cst_amt { get; set; } = 0m;
        public decimal? proj_f_fee_amt { get; set; } = 0m;
        List<PlProjectPlan> ProjectPlans { get; set; }
        public List<AccountGroupSetupDTO> EmployeeLaborAccounts { get; set; }
        public List<AccountGroupSetupDTO> EmployeeNonLaborAccounts { get; set; }
        public List<AccountGroupSetupDTO> SunContractorLaborAccounts { get; set; }
        public List<AccountGroupSetupDTO> SubContractorNonLaborAccounts { get; set; }
        public List<AccountGroupSetupDTO> OtherDirectCostNonLaborAccounts { get; set; }
        public List<AccountGroupSetupDTO> OtherDirectCostLaborAccounts { get; set; }
        public List<PlcCodeDTO> Plc { get; set; }


    }

    public class forecast
    {
        public int Pl_ID { get; set; }
        public string Empl_Id { get; set; }
        public int Emple_Id { get; set; }        
        public virtual PlEmployee? Empl { get; set; }
        public virtual PlEmployeee? Emple { get; set; }
        //public List<forecostHours>? emplHrs { get; set; }
    }

    public class DirectCostforecast
    {
        public int Pl_ID { get; set; }
        public int DctId { get; set; }
        public virtual PlDct? Empl { get; set; }
    }
    public class forecostHours
    {
        public int ForecastId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string displayText { get; set; }
        public decimal ForecastedHours { get; set; }
        public decimal? ForecastedAmount { get; set; }

    }
}
