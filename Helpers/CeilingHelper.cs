using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using PlanningAPI.DTO;
using PlanningAPI.Models;

public class CeilingHelper
{
    private readonly MydatabaseContext _context;

    public CeilingHelper(MydatabaseContext context) => _context = context;

    public async Task<StatusResponse<PlCeilBurden>> CreateBurdenCeilingForProjectAsync(PlCeilBurden plCeilBurden, string updatedBy)
    {
        await _context.PlCeilBurdens.AddAsync(plCeilBurden);
        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilBurden>(true, "Configuration Saved", plCeilBurden);
    }

    public async Task<StatusResponse<PlCeilBurden>> UpdateBurdenCeilingForProjectAsync(PlCeilBurden plCeilBurden, string updatedBy)
    {
        var existing = await _context.PlCeilBurdens
            .FirstOrDefaultAsync(p =>
                p.ProjectId == plCeilBurden.ProjectId &&
                p.AccountId == plCeilBurden.AccountId &&
                p.FiscalYear == plCeilBurden.FiscalYear);

        if (existing == null)
            return new StatusResponse<PlCeilBurden>(false, "Configuration Not Found", null);

        existing.RateCeiling = plCeilBurden.RateCeiling;
        existing.RateFormat = plCeilBurden.RateFormat;
        existing.ApplyToRbaCode = plCeilBurden.ApplyToRbaCode;

        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilBurden>(true, "Configuration Updated", existing);
    }

    public async Task<StatusResponse<List<PlCeilBurden>>> GetAllBurdenCeilingForProjectAsync(string projId)
    {
        var data = await (from cb in _context.PlCeilBurdens
                          join acc in _context.Accounts
                              on cb.AccountId equals acc.AcctId
                          where cb.ProjectId == projId
                          select new PlCeilBurden
                          {
                              FiscalYear = cb.FiscalYear,
                              ProjectId = cb.ProjectId,
                              AccountId = cb.AccountId,
                              RateCeiling = cb.RateCeiling,
                              ComCeiling = cb.ComCeiling,
                              ApplyToRbaCode = cb.ApplyToRbaCode,
                              ComFormat = cb.ComFormat,
                              CeilingMethodCode = cb.CeilingMethodCode,
                              PoolCode = cb.PoolCode,
                              AccountDesc = acc.AcctName,
                              RateFormat = cb.RateFormat
                          })
                          .ToListAsync();

        return new StatusResponse<List<PlCeilBurden>>(true, "success", data);
    }

    public async Task<StatusResponse<bool>> DeleteBurdenCeilingForProjectAsync(string projectId, string fiscalYear, string accountId, string poolCode)
    {
        var entity = await _context.PlCeilBurdens
            .FirstOrDefaultAsync(c => c.ProjectId == projectId
                                   && c.FiscalYear == fiscalYear
                                   && c.AccountId == accountId
                                   && c.PoolCode == poolCode);

        if (entity == null)
            return new StatusResponse<bool>(false, "Record Not Found", false);

        _context.PlCeilBurdens.Remove(entity);
        await _context.SaveChangesAsync();

        return new StatusResponse<bool>(true, "Record deleted successfully", true);
    }

    public async Task<StatusResponse<PlCeilHrCat>> CreateCeilingHrForPLCAsync(PlCeilHrCat plCeilHrCat, string updatedBy)
    {
        await _context.PlCeilHrCats.AddAsync(plCeilHrCat);
        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilHrCat>(true, "Configuration Saved", plCeilHrCat);
    }

    public async Task<StatusResponse<PlCeilHrCat>> UpdateCeilingHrForPLCAsync(PlCeilHrCat plCeilHrCat, string updatedBy)
    {
        var existing = await _context.PlCeilHrCats
            .FirstOrDefaultAsync(p =>
                p.ProjectId == plCeilHrCat.ProjectId &&
                p.LaborCategoryId == plCeilHrCat.LaborCategoryId);

        if (existing == null)
            return new StatusResponse<PlCeilHrCat>(false, "Configuration Not Found", null);

        existing.HoursCeiling = plCeilHrCat.HoursCeiling;
        existing.ApplyToRbaCode = plCeilHrCat.ApplyToRbaCode;

        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilHrCat>(true, "Configuration Updated", existing);
    }

    public async Task<StatusResponse<List<PlCeilHrCat>>> GetAllCeilingHrForPLCAsync(string projId)
    {
        //var data = await _context.PlCeilHrCats
        //    .Where(p => p.ProjectId == projId)
        //    .ToListAsync();

        var data = await (from hc in _context.PlCeilHrCats
                          join plc in _context.PlcCodes
                              on hc.LaborCategoryId equals plc.LaborCategoryCode
                          where hc.ProjectId == projId
                          select new PlCeilHrCat
                          {
                              ApplyToRbaCode = hc.ApplyToRbaCode,
                              HoursCeiling = hc.HoursCeiling,
                              LaborCategoryId = hc.LaborCategoryId,
                              LaborCategoryDesc = plc.Description ?? string.Empty,
                              ProjectId = hc.ProjectId
                          })
                  .ToListAsync();


        return new StatusResponse<List<PlCeilHrCat>>(true, "success", data);
    }

    public async Task<StatusResponse<bool>> DeleteCeilingHrForPLCAsync(string projectId, string laborCategoryId)
    {
        var entity = await _context.PlCeilHrCats
            .FirstOrDefaultAsync(c => c.ProjectId == projectId
                                   && c.LaborCategoryId == laborCategoryId);

        if (entity == null)
            return new StatusResponse<bool>(false, "Record Not Found", false);

        _context.PlCeilHrCats.Remove(entity);
        await _context.SaveChangesAsync();

        return new StatusResponse<bool>(true, "Record deleted successfully", true);
    }
    public async Task<StatusResponse<PlCeilHrEmpl>> CreateCeilingHrForEmpAsync(PlCeilHrEmpl plCeilHrEmpl, string updatedBy)
    {
        await _context.PlCeilHrEmpls.AddAsync(plCeilHrEmpl);
        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilHrEmpl>(true, "Configuration Saved", plCeilHrEmpl);
    }

    public async Task<StatusResponse<PlCeilHrEmpl>> UpdateCeilingHrForEmpAsync(PlCeilHrEmpl plCeilHrCat, string updatedBy)
    {
        var existing = await _context.PlCeilHrEmpls
            .FirstOrDefaultAsync(p =>
                p.ProjectId == plCeilHrCat.ProjectId &&
                p.LaborCategoryId == plCeilHrCat.LaborCategoryId);

        if (existing == null)
            return new StatusResponse<PlCeilHrEmpl>(false, "Configuration Not Found", null);

        existing.HoursCeiling = plCeilHrCat.HoursCeiling;
        existing.ApplyToRbaCode = plCeilHrCat.ApplyToRbaCode;

        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilHrEmpl>(true, "Configuration Updated", existing);
    }

    public async Task<StatusResponse<List<PlCeilHrEmpl>>> GetAllCeilingHrForEmpAsync(string projId)
    {
        //var data = await _context.PlCeilHrEmpls
        //    .Where(p => p.ProjectId == projId)
        //    .ToListAsync();

        var sql = @"
    SELECT 
        ce.labor_category_id AS ""LaborCategoryId"",
        COALESCE(plc.""DESCRIPTION"", '') AS ""LaborCategoryDesc"",
        ce.employee_id AS ""EmployeeId"",
        COALESCE(e.first_name, '') AS ""EmployeeName"",
        ce.hours_ceiling AS ""HoursCeiling"",
        ce.apply_to_rba_code AS ""ApplyToRbaCode"",
        ce.project_id AS ""ProjectId""
    FROM public.pl_ceil_hr_empl ce
    LEFT JOIN public.empl e 
        ON ce.employee_id = e.empl_id
    LEFT JOIN public.""PLC_CODES"" plc 
        ON ce.labor_category_id = plc.""PLC_CODE""
    WHERE ce.project_id = @projId";

        var data = await _context.Database.GetDbConnection()
            .QueryAsync<PlCeilHrEmpl>(sql, new { projId });

        return new StatusResponse<List<PlCeilHrEmpl>>(true, "success", data.ToList());
    }

    public async Task<StatusResponse<bool>> DeleteCeilingHrForEmpAsync(string projectId, string employeeId, string laborCategoryId)
    {
        var entity = await _context.PlCeilHrEmpls
            .FirstOrDefaultAsync(c => c.ProjectId == projectId
                                   && c.EmployeeId == employeeId
                                   && c.LaborCategoryId == laborCategoryId);

        if (entity == null)
            return new StatusResponse<bool>(false, "Record Not Found", false);

        _context.PlCeilHrEmpls.Remove(entity);
        await _context.SaveChangesAsync();

        return new StatusResponse<bool>(true, "Record deleted successfully", true);
    }

    public async Task<StatusResponse<PlCeilDirCst>> CreateCeilingAmtForDirectCostAsync(PlCeilDirCst plCeilDirCst, string updatedBy)
    {
        await _context.PlCeilDirCsts.AddAsync(plCeilDirCst);
        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilDirCst>(true, "Configuration Saved", plCeilDirCst);
    }

    public async Task<StatusResponse<PlCeilDirCst>> UpdateCeilingAmtForDirectCostAsync(PlCeilDirCst plCeilDirCst, string updatedBy)
    {
        var existing = await _context.PlCeilDirCsts
            .FirstOrDefaultAsync(p =>
                p.ProjectId == plCeilDirCst.ProjectId &&
                p.AccountId == plCeilDirCst.AccountId);

        if (existing == null)
            return new StatusResponse<PlCeilDirCst>(false, "Configuration Not Found", null);

        existing.CeilingAmountFunc = plCeilDirCst.CeilingAmountFunc;
        existing.CeilingAmountBilling = plCeilDirCst.CeilingAmountBilling;
        existing.ApplyToRbaCode = plCeilDirCst.ApplyToRbaCode;

        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilDirCst>(true, "Configuration Updated", existing);
    }

    public async Task<StatusResponse<List<PlCeilDirCst>>> GetAllCeilingAmtForDirectCostAsync(string projId)
    {
        //var data = await _context.PlCeilDirCsts
        //    .Where(p => p.ProjectId == projId)
        //    .ToListAsync();

        var data = await (from cb in _context.PlCeilDirCsts
                          join acc in _context.Accounts
                              on cb.AccountId equals acc.AcctId
                          where cb.ProjectId == projId
                          select new PlCeilDirCst
                          {
                              ProjectId = cb.ProjectId,
                              AccountId = cb.AccountId,
                              ApplyToRbaCode = cb.ApplyToRbaCode,
                              AccountDesc = acc.AcctName,
                              CeilingAmountBilling = cb.CeilingAmountBilling,
                              CeilingAmountFunc = cb.CeilingAmountFunc
                          })
                  .ToListAsync();

        return new StatusResponse<List<PlCeilDirCst>>(true, "success", data);
    }

    public async Task<StatusResponse<bool>> DeleteAllCeilingAmtForDirectCostAsync(string projectId, string accountId)
    {
        var entity = await _context.PlCeilDirCsts
            .FirstOrDefaultAsync(c => c.ProjectId == projectId && c.AccountId == accountId);

        if (entity == null)
            return new StatusResponse<bool>(false, "Record Not Found", false);

        _context.PlCeilDirCsts.Remove(entity);
        await _context.SaveChangesAsync();

        return new StatusResponse<bool>(true, "Record deleted successfully", true);
    }



    public async Task<StatusResponse<PlCeilProjTotal>> CreateCeilingAmtForTotalProjectCostAsync(PlCeilProjTotal plCeilProjTotal, string updatedBy)
    {
        await _context.PlCeilProjTotals.AddAsync(plCeilProjTotal);
        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilProjTotal>(true, "Configuration Saved", plCeilProjTotal);
    }

    public async Task<StatusResponse<PlCeilProjTotal>> UpdateCeilingAmtForTotalProjectCostAsync(PlCeilProjTotal plCeilProjTotal, string updatedBy)
    {
        var existing = await _context.PlCeilProjTotals
            .FirstOrDefaultAsync(p =>
                p.ProjectId == plCeilProjTotal.ProjectId);

        if (existing == null)
            return new StatusResponse<PlCeilProjTotal>(false, "Configuration Not Found", null);

        existing.TotalValueCeiling = plCeilProjTotal.TotalValueCeiling;
        existing.CostCeiling = plCeilProjTotal.CostCeiling;
        existing.FeeCeiling = plCeilProjTotal.FeeCeiling;
        existing.CeilingCode = plCeilProjTotal.CeilingCode;

        await _context.SaveChangesAsync();

        return new StatusResponse<PlCeilProjTotal>(true, "Configuration Updated", existing);
    }

    public async Task<StatusResponse<List<PlCeilProjTotal>>> GetAllCeilingAmtForTotalProjectCostAsync(string projId)
    {
        var data = await _context.PlCeilProjTotals
            .Where(p => p.ProjectId == projId)
            .ToListAsync();

        return new StatusResponse<List<PlCeilProjTotal>>(true, "success", data);
    }

    public async Task<StatusResponse<bool>> DeleteCeilingAmtForTotalProjectCostAsync(string proj_id)
    {
        var entity = await _context.PlCeilProjTotals.FindAsync(proj_id);

        if (entity == null)
            return new StatusResponse<bool>(false, $"Record with Id {proj_id} not found", false);

        _context.PlCeilProjTotals.Remove(entity);
        await _context.SaveChangesAsync();

        return new StatusResponse<bool>(true, "Record deleted successfully", true);
    }

    public List<PlWarning> GetWarningsByPlId(int plId)
    {
        //var warnings = _context.PLWarnings
        //                .FromSqlRaw(@"SELECT pl_id as PlId, proj_id as ProjId, empl_id as EmplId, year, month, warning 
        //                                  FROM pl_warnings 
        //                                  WHERE pl_id = {0}", plId).ToList();
        var warnings = _context.PLWarnings.Where(w => w.PlId == plId).ToList();
        return warnings;
    }

    public List<PlWarning> GetWarningsByEmployee(int plId,string empl_id)
    {
        //var warnings = _context.PLWarnings
        //                .FromSqlRaw(@"SELECT pl_id as PlId, proj_id as ProjId, empl_id as EmplId, year, month, warning 
        //                                  FROM pl_warnings 
        //                                  WHERE empl_id = {0} and multiple_projects = true", empl_id).ToList();

        var warnings = _context.PLWarnings.Where(w => w.EmplId == empl_id && w.PlId == plId).ToList();
        return warnings;
    }

}
