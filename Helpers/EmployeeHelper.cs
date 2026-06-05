using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Services;

public class EmployeeHelper
{
    public bool ValidateEmployee(string employeeId, IEmplService emplService)
    {
        return emplService.GetEployeeByID(employeeId) != null;
    }

    //public List<EmplSchedule> CalculateSalaryPlan(List<PlForecast> forecasts, decimal baseSalary, int startYear, int incrementMonth, decimal incrementRate)
    //{
    //    var proj = forecasts.First().Proj;
    //    int durationMonths = ((proj.ProjEndDt.Value.Year - proj.ProjStartDt.Value.Year) * 12) +
    //                         proj.ProjEndDt.Value.Month - proj.ProjStartDt.Value.Month + 1;

    //    var schedules = new List<EmplSchedule>();
    //    foreach (var empId in forecasts.Select(f => f.EmplId).Distinct())
    //    {
    //        var emp = forecasts.First(f => f.EmplId == empId).Empl;
    //        baseSalary = emp.PerHourRate ?? baseSalary;
    //        int effectiveIncrementMonth = incrementMonth == 0 ? emp.HireDate.Value.Month : incrementMonth;

    //        var monthlyPlan = SalaryHelper.CalculatePerhrSalaryPlan(
    //            baseSalary,
    //            proj.ProjStartDt.Value.Year,
    //            proj.ProjStartDt.Value.Month,
    //            effectiveIncrementMonth,
    //            incrementRate,
    //            durationMonths
    //        );

    //        schedules.Add(new EmplSchedule { EmpId = empId, monthlySalary = monthlyPlan });
    //    }

    //    return schedules;
    //}
}
