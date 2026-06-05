using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NPOI.HSSF.Record;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Models;
using System;
using WebApi.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static WebApi.DTO.EmployeeDTOs;

public class FinanceHelper
{
    private readonly MydatabaseContext _context;
    private readonly List<ProjectPlcRate> rate;
    private readonly List<ProjEmplRt> employeeRate;

    public FinanceHelper(MydatabaseContext context, string projId)
    {
        _context = context;

        rate = _context.ProjectPlcRates
                .Where(r => r.ProjId.StartsWith(projId) && r.IsActive)
                .OrderByDescending(r => r.EffectiveDate).ToList();

        employeeRate = _context.ProjEmplRts
                .Where(r => r.ProjId.StartsWith(projId))
                .OrderByDescending(r => r.EndDt).ToList();
    }

    public decimal CalculateBurdenedCost(BurdenInput input)
    {
        decimal baseCost = 0;
        if (input.HourlyRate != 0)
            baseCost = input.Hours * input.HourlyRate;
        else
            baseCost = input.DirectCost;

        return baseCost
                * (1 + input.FringeRate / 100)
                * (1 + input.OverheadRate / 100)
                * (1 + input.GnaRate / 100);
    }

    public decimal CalculateBurdenedCostBYRAPRate(BurdenInput input)
    {
        decimal baseCost = 0;
        if (input.HourlyRate != 0)
            baseCost = input.Hours * input.HourlyRate;
        else
            baseCost = input.DirectCost;

        var fringe = baseCost * (input.FringeRate / 100);
        var overHead = baseCost * (input.OverheadRate / 100);

        baseCost = baseCost + fringe + overHead;
        return baseCost
            * (1 + input.GnaRate / 100);
    }

    public decimal GetActiveFeesForProject(string projId)
    {
        var today = DateTime.UtcNow;
        var fee = _context.ProjectFees
            .Where(f => f.ProjId == projId && f.IsActive && f.EffectiveDate <= today &&
                        (f.EndDate == null || f.EndDate >= today))
            .OrderByDescending(f => f.EffectiveDate)
            .FirstOrDefault();

        return fee?.FeeType switch
        {
            "PERCENTAGE" => fee.FeePercent ?? 0,
            "FIXED" => fee.FixedAmount ?? 0,
            _ => 0
        };
    }

    public async Task<decimal> CalculateRevenueTNMAsync(string projId, string plcCode, decimal hours, int Month, int Year, string emplId)
    {

        DateTime monthEndDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month)).ToUniversalTime();
        decimal billingRate = 0;
        var emplrate = this.employeeRate.Where(r => r.EmplId == emplId &&
                r.StartDt <= monthEndDate)
                .OrderByDescending(r => r.StartDt)
                .FirstOrDefault();


        if (emplrate == null)
        {
            var rate = this.rate.Where(r => r.ProjId.StartsWith(projId) && r.LaborCategoryCode == plcCode && r.IsActive &&
                            r.EffectiveDate <= monthEndDate)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefault();

            if (rate != null)
                billingRate = rate.BillingRate.GetValueOrDefault();
        }
        else
        {
            billingRate = emplrate.BillRtAmt.GetValueOrDefault();
        }

        return billingRate * hours;
    }

    public List<MonthlySalary> CalculatePerhrSalaryPlan(decimal initialSalary, int startYear, int startMonth, int incrementMonth, decimal incrementRate, int totalMonths, List<EmployeeDTOs> emplperHrRateList)
    {
        var salaryPlan = new List<MonthlySalary>();
        decimal currentSalary = initialSalary;
        decimal salRatio = 1;

        for (int i = 0; i < totalMonths; i++)
        {


            int totalElapsed = startMonth - 1 + i;
            int year = startYear + (totalElapsed / 12);
            int month = (totalElapsed % 12) + 1;

            if (year == 2025 && month == 3 && emplperHrRateList.FirstOrDefault()?.EmpId == "1003373")
            {

            }

            if (year == 2025 && month == 10 && emplperHrRateList.FirstOrDefault()?.EmpId == "1003699")
            {

            }

            //////////////////////////Add Condition
            if (year < DateTime.Now.Year ||
               (year == DateTime.Now.Year && month <= DateTime.Now.Month))
            {
                DateOnly date = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

                var records = emplperHrRateList
                            .Where(e => e.EffectiveDate <= date)
                            .OrderByDescending(e => e.EffectiveDate).ToList();

                var record = records.FirstOrDefault();
                if (record != null)
                {
                    bool isMidMonth =
                            record.EffectiveDate.GetValueOrDefault().Day != 1 &&
                            record.EffectiveDate.GetValueOrDefault().Day != DateTime.DaysInMonth(record.EffectiveDate.GetValueOrDefault().Year, record.EffectiveDate.GetValueOrDefault().Month);

                    if (isMidMonth && record.EffectiveDate.GetValueOrDefault().Year == year && record.EffectiveDate.GetValueOrDefault().Month == month)
                    {
                        try
                        {
                            int totalDaysInMonth = DateTime.DaysInMonth(year, month);
                            decimal oldRate = 0, newRate = 0;
                            // Example: first 10 days have old rate
                            var startDate = new DateOnly(year, month, 1);
                            var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

                            int days = record.EffectiveDate.GetValueOrDefault().DayNumber - startDate.DayNumber + 1;
                            //int days = (record.EffectiveDate.GetValueOrDefault() - startDate).Days;
                            //int daysAtOldRate = record.EffectiveDate.GetValueOrDefault().DayNumber - startDate.DayNumber + 1;

                            /////////////////////
                            int daysAtOldRate = record.EffectiveDate.GetValueOrDefault().DayNumber - startDate.DayNumber;
                            int daysAtNewRate = DateTime.DaysInMonth(year, month) - daysAtOldRate;

                            if (records != null && records.Count > 2)
                            {
                                oldRate = records[1].HrRate.GetValueOrDefault();
                                newRate = records[0].HrRate.GetValueOrDefault();
                                currentSalary =
                                    ((oldRate * daysAtOldRate) + (newRate * daysAtNewRate))
                                    / totalDaysInMonth;
                            }
                            else
                            {
                                currentSalary = record.HrRate.GetValueOrDefault();
                            }

                        }
                        catch (Exception ex)
                        {
                        }
                        //records = records
                        //        .Where(e => e.EffectiveDate.GetValueOrDefault().Day == 1)
                        //        .OrderByDescending(e => e.EffectiveDate).ToList();

                    }
                    else
                        currentSalary = record.HrRate.GetValueOrDefault();
                }

            }
            else
            {
                if (month == incrementMonth && i != 0)
                {
                    currentSalary += currentSalary * (incrementRate / 100);
                    salRatio += salRatio * (incrementRate / 100);
                }
            }

            salaryPlan.Add(new MonthlySalary { Year = year, Month = month, Salary = Math.Round(currentSalary, 6), SalRatio = Math.Round(salRatio, 2) });

        }

        return salaryPlan;
    }


    public List<MonthlySalary> CalculatePerhrSalaryPlanForVendor(decimal initialSalary, int startYear, int startMonth, int incrementMonth, decimal incrementRate, int totalMonths, decimal perHrRate)
    {
        var salaryPlan = new List<MonthlySalary>();
        decimal currentSalary = initialSalary;
        decimal salRatio = 1;

        for (int i = 0; i < totalMonths; i++)
        {
            int totalElapsed = startMonth - 1 + i;
            int year = startYear + (totalElapsed / 12);
            int month = (totalElapsed % 12) + 1;
            DateOnly date = new DateOnly(year, month, 1);
            currentSalary = perHrRate;
            salaryPlan.Add(new MonthlySalary { Year = year, Month = month, Salary = Math.Round(currentSalary, 2), SalRatio = Math.Round(salRatio, 2) });
        }

        return salaryPlan;
    }

    public List<EmplSchedule> CalculateSalaryPlan(List<PlForecast> plForecast, decimal initialSalary, int startYear, int increamentMonth, decimal incrementRate, PlProjectPlan projectPlan)
    {
        int projectDurationMonths = (projectPlan.ProjEndDt.GetValueOrDefault().Year -
                                    projectPlan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                                    projectPlan.ProjEndDt.GetValueOrDefault().Month -
                                    projectPlan.ProjStartDt.GetValueOrDefault().Month + 1;

        List<EmplSchedule> emplSchedules = new List<EmplSchedule>();

        var distinctEmployees = plForecast.Select(p => new { p.EmplId, p.AcctId, p.Plc, p.OrgId }).Distinct().ToList();

        //_context.PlEmployeees.Where()


        var emplsperHrRateList = new List<EmployeeDTOs>();
        if (distinctEmployees.Any())
        {
            emplsperHrRateList = _context.EmployeeDTOs
                .FromSqlRaw(@"SELECT  
                                a.empl_id AS EmpId, 
                                '' AS EmployeeName,
                                '' AS OrgId,
                                '' AS Plc,
                                a.hrly_amt AS HrRate,
                                a.effect_dt AS EffectiveDate,
								 '50-000-000' AS AcctId,
								  'Direct Lbr-Onsite' AS AcctName,
								'' As OrgName,
                                a.lab_grp_type As LaborGroup
                            FROM public.empl_lab_info a
                            WHERE a.empl_id in (" + string.Join(",", distinctEmployees.Select(x => $"'{x.EmplId}'")) + ")")
                .ToList();

        }
        var salaryPlan = new List<MonthlySalary>();
        foreach (var empl in distinctEmployees)
        {
            EmplSchedule emplSchedule = new EmplSchedule();
            emplSchedule.payrollSalary = new List<WebApi.DTO.MonthlySalary>();
            //var employee = plForecast.FirstOrDefault(p => p.EmplId == emplid).Emple;
            var employee = plForecast.FirstOrDefault(p =>
                               p.EmplId == empl.EmplId &&
                               p.AcctId == empl.AcctId &&
                               p.Plc == empl.Plc &&
                               p.OrgId == empl.OrgId
                           )?.Emple;
            initialSalary = employee.PerHourRate.GetValueOrDefault();
            emplSchedule.EmpId = empl.EmplId;
            emplSchedule.PlcCode = employee.PlcGlcCode;
            emplSchedule.AccID = employee.AccId;
            emplSchedule.OrgID = employee.OrgId;
            emplSchedule.isRev = employee.IsRev.GetValueOrDefault();

            var emplperHrRateList = emplsperHrRateList
            .Where(e => e.EmpId == emplSchedule.EmpId).ToList();

            if (employee.Type != null && (employee.Type.ToUpper() == "VENDOREMPLOYEE" || employee.Type.ToUpper() == "VENDOR EMPLOYEE"))
            {
                emplSchedule.payrollSalary = CalculatePerhrSalaryPlanForVendor(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, employee.HireDate.GetValueOrDefault().Month, 0, projectDurationMonths, employee.PerHourRate.GetValueOrDefault());
                emplSchedules.Add(emplSchedule);
                continue;
            }
            else
            {


                if (emplSchedule.EmpId == "1004061")
                {

                }
                if (increamentMonth == 0)
                {
                    emplSchedule.payrollSalary = CalculatePerhrSalaryPlan(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, employee.HireDate.GetValueOrDefault().Month, incrementRate, projectDurationMonths, emplperHrRateList);
                    emplSchedules.Add(emplSchedule);
                }
                else
                {
                    emplSchedule.payrollSalary = CalculatePerhrSalaryPlan(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, increamentMonth, incrementRate, projectDurationMonths, emplperHrRateList);
                    emplSchedules.Add(emplSchedule);
                }
            }
        }

        return emplSchedules;
    }


    public List<EmplSchedule> CalculateSalaryPlanV1(List<PlForecast> plForecast, decimal initialSalary, int startYear, int increamentMonth, decimal incrementRate, PlProjectPlan projectPlan)
    {
        int projectDurationMonths = (projectPlan.ProjEndDt.GetValueOrDefault().Year -
                                    projectPlan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                                    projectPlan.ProjEndDt.GetValueOrDefault().Month -
                                    projectPlan.ProjStartDt.GetValueOrDefault().Month + 1;

        List<EmplSchedule> emplSchedules = new List<EmplSchedule>();

        var distinctEmployees = plForecast.Select(p => new { p.EmplId, p.AcctId, p.Plc, p.OrgId, p.Emple.Esc_Percent }).Distinct().ToList();

        //_context.PlEmployeees.Where()


        var emplsperHrRateList = new List<EmployeeDTOs>();
        if (distinctEmployees.Any())
        {
            emplsperHrRateList = _context.EmployeeDTOs
                .FromSqlRaw(@"SELECT  
                                a.empl_id AS EmpId, 
                                '' AS EmployeeName,
                                '' AS OrgId,
                                '' AS Plc,
                                a.hrly_amt AS HrRate,
                                a.effect_dt AS EffectiveDate,
								 '50-000-000' AS AcctId,
								  'Direct Lbr-Onsite' AS AcctName,
								'' As OrgName,
                            a.lab_grp_type As LaborGroup
                            FROM public.empl_lab_info a
                            WHERE a.empl_id in (" + string.Join(",", distinctEmployees.Select(x => $"'{x.EmplId}'")) + ")")
                .ToList();

        }
        var salaryPlan = new List<MonthlySalary>();
        foreach (var empl in distinctEmployees)
        {
            EmplSchedule emplSchedule = new EmplSchedule();
            emplSchedule.payrollSalary = new List<WebApi.DTO.MonthlySalary>();
            //var employee = plForecast.FirstOrDefault(p => p.EmplId == emplid).Emple;
            var employee = plForecast.FirstOrDefault(p =>
                               p.EmplId == empl.EmplId &&
                               p.AcctId == empl.AcctId &&
                               p.Plc == empl.Plc &&
                               p.OrgId == empl.OrgId
                           )?.Emple;
            initialSalary = employee.PerHourRate.GetValueOrDefault();
            emplSchedule.EmpId = empl.EmplId;
            emplSchedule.PlcCode = employee.PlcGlcCode;
            emplSchedule.AccID = employee.AccId;
            emplSchedule.OrgID = employee.OrgId;
            emplSchedule.isRev = employee.IsRev.GetValueOrDefault();

            if(employee.EffectiveDate.HasValue)
            {
                increamentMonth = employee.EffectiveDate.Value.Month;
            }
            

            var emplperHrRateList = emplsperHrRateList
            .Where(e => e.EmpId == emplSchedule.EmpId).ToList();

            if (employee.Type != null && (employee.Type.ToUpper() == "VENDOREMPLOYEE" || employee.Type.ToUpper() == "VENDOR EMPLOYEE"))
            {
                emplSchedule.payrollSalary = CalculatePerhrSalaryPlanForVendor(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, employee.HireDate.GetValueOrDefault().Month, 0, projectDurationMonths, employee.PerHourRate.GetValueOrDefault());
                emplSchedules.Add(emplSchedule);
                continue;
            }
            else
            {
                if (empl != null && empl?.Esc_Percent != null)
                {
                    incrementRate = empl.Esc_Percent.GetValueOrDefault();
                }
                if (emplSchedule.EmpId == "1004061")
                {

                }
                if (increamentMonth == 0)
                {
                    emplSchedule.payrollSalary = CalculatePerhrSalaryPlan(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, employee.HireDate.GetValueOrDefault().Month, incrementRate, projectDurationMonths, emplperHrRateList);
                    emplSchedules.Add(emplSchedule);
                }
                else
                {
                    emplSchedule.payrollSalary = CalculatePerhrSalaryPlan(initialSalary, projectPlan.ProjStartDt.GetValueOrDefault().Year, projectPlan.ProjStartDt.GetValueOrDefault().Month, increamentMonth, incrementRate, projectDurationMonths, emplperHrRateList);
                    emplSchedules.Add(emplSchedule);
                }
            }
        }

        return emplSchedules;
    }

    public List<DirectCostSchedule> CalculateDirectCostPlan(List<PlForecast> plForecast)
    {

        var distinctDirectCost = plForecast.Select(p => p.DctId).Distinct().ToList();
        List<DirectCostSchedule> directCostSchedules = new List<DirectCostSchedule>();


        foreach (var DirectCostId in distinctDirectCost)
        {
            DirectCostSchedule directCostSchedule = new DirectCostSchedule();
            var DirectCost = plForecast.FirstOrDefault(p => p.DctId == DirectCostId.GetValueOrDefault()).DirectCost;
            directCostSchedule.DctId = DirectCostId.GetValueOrDefault();
            directCostSchedule.AcctId = DirectCost.AcctId;
            directCostSchedule.OrgID = DirectCost.OrgId;
            directCostSchedule.isRev = DirectCost.IsRev;
            directCostSchedule.forecasts = plForecast.Where(p => p.DctId == DirectCostId).OrderBy(p => p.Year).ThenBy(p => p.Month).ToList();
            directCostSchedules.Add(directCostSchedule);
        }

        return directCostSchedules;
    }


    internal Dictionary<string, decimal> GetBurdenRates(List<string> templatePools, List<PlTemplatePoolRate> burdens, string type)
    {
        Dictionary<string, decimal> lst = new Dictionary<string, decimal>();
        foreach (var burden in burdens)
        {
            if (templatePools.Contains(burden.PoolId))
            {
                if (type != "ACTUAL")
                    lst[burden.PoolId] = burden.TargetRate.GetValueOrDefault();
                else
                    lst[burden.PoolId] = burden.ActualRate.GetValueOrDefault();
            }
        }
        return lst;
    }
    public List<PlForecast> AdjustRevenue(List<PlForecast> items, decimal funding, PlProjectPlan projPlan)
    {
        decimal maxTotal = funding;
        decimal runningTotal = 0;

        if (projPlan.PlType == "EAC")
        {
            var actualMonthlySummary = _context.PSRFinalData
            .Where(p => p.ProjId.StartsWith(projPlan.ProjId) && (p.RateType == projPlan.Type || p.RateType == "N") && Convert.ToInt16(p.FyCd) == projPlan.ClosedPeriod.Value.Year && p.PdNo <= projPlan.ClosedPeriod.Value.Month)
            .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
            .Select(g => new MonthlySummary
            {
                Month = g.Key.PdNo,
                Year = Convert.ToInt16(g.Key.FyCd),
                Cost = g.Sum(x => x.PtdIncurAmt),
                subTotalType = g.Key.SubTotTypeNo
                //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

            })
            .ToList();

            runningTotal = actualMonthlySummary.Where(m => m.subTotalType == 1).Sum(m => m.Cost);
        }
        foreach (var item in items)
        {
            if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(item.Year, item.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
            {
                continue;
            }

            if (runningTotal >= maxTotal)
            {
                item.Revenue = 0;
            }
            else if (runningTotal + item.Revenue > maxTotal)
            {
                item.Revenue = maxTotal - runningTotal;
                runningTotal = maxTotal;
            }
            else
            {
                runningTotal += item.Revenue;
            }
        }

        return items;
    }
}
