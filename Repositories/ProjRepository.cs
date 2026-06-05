namespace WebApi.Repositories;

using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using System;
using WebApi.DTO;
using WebApi.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public interface IProjRepository
{

    Task<IEnumerable<ProjectDTO>> GetAllProjects();
    Task<IEnumerable<ProjectDTO>> GetAllProjects(int UserId);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(string proj_id);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(int UserId, string Role, string proj_id);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsByOrg(string orgId);
    //Task<List<PlConfigValue>> GetAllConfigValuesByProject(string projID);
    //Task<PlConfigValue> GetConfigValueByName(string name);
    //Task<PlConfigValue> AddConfigValue(PlConfigValue plConfigValue);
    //Task<bool> UpdateConfigValue(PlConfigValue plConfigValue);
    //Task<bool> DeleteConfigValueByName(string name);
    //Task<bool> DeleteConfigValueById(int id);
    Task<List<PlcCode>> GetAllPlcs(string? plc);
    Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId);
    Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId, string PlType);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForValidate(string projId);
}

public class ProjRepository : IProjRepository
{
    //private DataContext _context;
    private readonly MydatabaseContext _context;

    public ProjRepository(MydatabaseContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjects()
    {

        //var data = (from pe in _context.ProjEmpls
        //            join e in _context.Empls on pe.EmplId equals e.EmplId
        //            join eli in _context.EmplLabInfos on pe.EmplId equals eli.EmplId
        //            select new
        //            {
        //                EmployeeName = e.FirstName,
        //                ProjectId = pe.ProjId,
        //                HrRate = eli.HrlyAmt,
        //                EmpId = pe.EmplId

        //            }).ToList();

        var projects = _context.PlProjects.Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
            StartDate = h.ProjStartDt,
            EndDate = h.ProjEndDt,
            Description = h.Notes,
            OrgId = h.OrgId,
            Type = h.ProjTypeDc,
            proj_f_cst_amt = h.proj_f_cst_amt,
            proj_f_tot_amt = h.proj_f_tot_amt,
            proj_f_fee_amt = h.proj_f_fee_amt
        })
    .ToList();

        return Task.FromResult((IEnumerable<ProjectDTO>)projects);

    }

    public async Task<IEnumerable<ProjectDTO>> GetAllProjects(int User_Id)
    {
        var orgIds = await _context.OrgGroupUserMappings
            .Where(x => x.UserId == User_Id && x.IsActive)
            .SelectMany(x => x.OrgGroup.OrgMappings)
            .Select(m => m.OrgId)
            .Distinct()
            .ToArrayAsync();

        var projects = _context.PlProjects.Where(p => orgIds.Contains(p.OrgId)).Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
            StartDate = h.ProjStartDt,
            EndDate = h.ProjEndDt,
            Description = h.Notes,
            OrgId = h.OrgId
        }).ToList();
        return projects;


        //    var projects = _context.PlProjects.Select(h => new ProjectDTO
        //    {
        //        ProjectId = h.ProjId,
        //        Name = h.ProjName,
        //        StartDate = h.ProjStartDt,
        //        EndDate = h.ProjEndDt,
        //        Description = h.Notes,
        //        OrgId = h.OrgId,
        //        Type = h.ProjTypeDc,
        //        proj_f_cst_amt = h.proj_f_cst_amt,
        //        proj_f_tot_amt = h.proj_f_tot_amt,
        //        proj_f_fee_amt = h.proj_f_fee_amt
        //    })
        //.ToList();

        //    return projects;

    }
    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(string proj_id)
    {

        var projects = _context.PlProjects.Where(p => p.ProjId.ToUpper().StartsWith(proj_id.ToUpper())).Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
        })
        .ToList();
        return Task.FromResult((IEnumerable<ProjectDTO>)projects);
    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(int UserId, string Role, string? proj_id)
    {
        string[] projIds = Array.Empty<string>();
        projIds = _context.UserProjectMaps
           .Where(u => u.UserId == UserId)
           .Select(u => u.ProjId)
           .ToArray();

        var query = _context.PlProjects.AsQueryable();
        if (!Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(pp => projIds.Contains(pp.ProjId));
        }
        if (!string.IsNullOrEmpty(proj_id))
        {
            query = query.Where(p => p.ProjId.ToUpper().StartsWith(proj_id.ToUpper()));
        }

        var projects = query.Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
            Type = h.ProjTypeDc
        })
        .ToList();
        return Task.FromResult((IEnumerable<ProjectDTO>)projects);
    }
    public Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId, string PlType)
    {
        //var test = _context.AccountGroupSetup.FirstOrDefault(p => p.AcctGroupCode.ToUpper() == "DIR" && p.AccountFunctionDescription.ToUpper() == "REVENUE");
        List<ProjectDTO> projects = new List<ProjectDTO>();
        List<AccountGroupSetupDTO> accountGroupSetupDTOs = new List<AccountGroupSetupDTO>();

        var validDescriptions = new List<string> { "REVENUE", "NON-LABOR", "LABOR", "UNALLOW-LABOR" };
        if (PlType != "NBBUD")
        {
            projects = _context.PlProjects.Where(p => p.ProjId == ProjId).Select(h => new ProjectDTO
            {
                ProjectId = h.ProjId,
                Name = h.ProjName,
                StartDate = h.ProjStartDt,
                EndDate = h.ProjEndDt,
                Description = h.Notes,
                OrgId = h.OrgId,
                Type = h.ProjTypeDc,
                proj_f_cst_amt = h.proj_f_cst_amt,
                proj_f_tot_amt = h.proj_f_tot_amt,
                proj_f_fee_amt = h.proj_f_fee_amt,
                AccountGroupCode = h.AcctGrpCd
                //accounts = _context.AccountGroupSetup.Where(p=>p.AcctGroupCode == h.AcctGrpCd && validDescriptions.Contains(p.AccountFunctionDescription.ToUpper())).Select(p=> new AccountGroupSetupDTO { AccountId = p.AccountId, AccountFunctionDescription = p.AccountFunctionDescription}).ToList()
            })
            .ToList();


            if (projects != null && projects.Count() > 0)
            {
                var orgs = _context.Organizations.Where(p => projects.Select(q => q.OrgId).Contains(p.OrgId)).ToList();
                foreach (var project in projects)
                {
                    project.OrgName = orgs.FirstOrDefault(p => p.OrgId == project.OrgId)?.OrgName;
                }
            }

            accountGroupSetupDTOs = (from ags in _context.AccountGroupSetup
                                     join a in _context.Accounts
                                         on ags.AccountId equals a.AcctId
                                     where ags.AcctGroupCode == projects[0].AccountGroupCode
                                        && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
                                     select new AccountGroupSetupDTO
                                     {
                                         AccountId = ags.AccountId,
                                         AccountFunctionDescription = ags.AccountFunctionDescription,
                                         AcctName = a.AcctName
                                     }).ToList();
        }
        else
        {
            projects = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == ProjId).Select(h => new ProjectDTO
            {
                ProjectId = h.BusinessBudgetId,
                Name = h.Description,
                StartDate = DateOnly.FromDateTime(h.StartDate),
                EndDate = DateOnly.FromDateTime(h.EndDate),
                Description = h.Description,
                OrgId = h.OrgId.ToString(),
                Type = "",
                AccountGroupCode = h.AccountGroup
                //proj_f_cst_amt = h.proj_f_cst_amt,
                //proj_f_tot_amt = h.proj_f_tot_amt,
                //proj_f_fee_amt = h.proj_f_fee_amt,
                //AccountGroupCode = h.AcctGrpCd
                //accounts = _context.AccountGroupSetup.Where(p=>p.AcctGroupCode == h.AcctGrpCd && validDescriptions.Contains(p.AccountFunctionDescription.ToUpper())).Select(p=> new AccountGroupSetupDTO { AccountId = p.AccountId, AccountFunctionDescription = p.AccountFunctionDescription}).ToList()
            }).ToList();


            if (projects != null && projects.Count() > 0)
            {
                var orgs = _context.Organizations.Where(p => projects.Select(q => q.OrgId).Contains(p.OrgId)).ToList();
                foreach (var project in projects)
                {
                    project.OrgName = orgs.FirstOrDefault(p => p.OrgId == project.OrgId)?.OrgName;
                }
            }

            accountGroupSetupDTOs = (from ags in _context.AccountGroupSetup
                                     join a in _context.Accounts
                                         on ags.AccountId equals a.AcctId
                                     where ags.AcctGroupCode == projects[0].AccountGroupCode && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
                                     select new AccountGroupSetupDTO
                                     {
                                         AccountId = ags.AccountId,
                                         AccountFunctionDescription = ags.AccountFunctionDescription,
                                         AcctName = a.AcctName
                                     }).ToList();

        }
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();

        var Allplcs = _context.PlcCodes.ToList();
        //var test = _context.AccountGroupSetup.Where(p => p.AcctGroupCode == projects[0].AccountGroupCode && validDescriptions.Contains(p.AccountFunctionDescription.ToUpper())).Select(p => new AccountGroupSetupDTO { AccountId = p.AccountId, AccountFunctionDescription = p.AccountFunctionDescription }).ToList();

        foreach (var account in accountGroupSetupDTOs)
        {
            var acc = chartOfAccounts.Where(p => string.Join("-", p.AccountId.Split('-').Take(2)) == string.Join("-", account.AccountId.Split("-").Take(2))).FirstOrDefault();
            if (acc != null)
                account.BudgetSheet = acc.BudgetSheet;
        }

        projects[0].RevenueAccount = accountGroupSetupDTOs.FirstOrDefault(p => p.AccountFunctionDescription == "REVENUE")?.AccountId;
        projects[0].EmployeeLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "LABOR" && (p.BudgetSheet?.ToUpper() == "Employee".ToUpper() || p.BudgetSheet?.ToUpper() == "Staff Hours".ToUpper())).ToList();
        projects[0].EmployeeLaborAccounts.AddRange(accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "UNALLOW-LABOR" && (p.BudgetSheet?.ToUpper() == "Employee".ToUpper() || p.BudgetSheet?.ToUpper() == "Staff Hours".ToUpper())).ToList());
        projects[0].EmployeeNonLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "NON-LABOR" && (p.BudgetSheet?.ToUpper() == "Employee".ToUpper() || p.BudgetSheet?.ToUpper() == "Staff Hours".ToUpper())).ToList();
        projects[0].SunContractorLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "LABOR" && (p.BudgetSheet?.ToUpper() == ("Subcontractor Hours".ToUpper()) || p.BudgetSheet?.ToUpper() == "Subcontractors".ToUpper())).ToList();
        projects[0].SunContractorLaborAccounts.AddRange(accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "UNALLOW-LABOR" && (p.BudgetSheet?.ToUpper() == "Subcontractor Hours".ToUpper() || p.BudgetSheet?.ToUpper() == "Subcontractors".ToUpper())).ToList());
        projects[0].SubContractorNonLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "NON-LABOR" && (p.BudgetSheet?.ToUpper() == ("Subcontractor Hours".ToUpper()) || p.BudgetSheet?.ToUpper() == "Subcontractors".ToUpper())).ToList();
        projects[0].OtherDirectCostNonLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "NON-LABOR" && p.BudgetSheet?.ToUpper() == "Other Direct Costs".ToUpper()).ToList();
        projects[0].OtherDirectCostLaborAccounts = accountGroupSetupDTOs.Where(p => p.AccountFunctionDescription.ToUpper() == "LABOR" && p.BudgetSheet?.ToUpper() == "Other Direct Costs".ToUpper()).ToList();

        string workforce = "false";
        try
        {
            workforce = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "workforce" && r.ProjId == ProjId)?.Value ?? "false";
        }
        catch (Exception ex) { }

        if (workforce.ToUpper() == "TRUE")
        {


            projects[0].Plc = (
                from labcat in _context.ProjEmployeeLabcats
                join plc in _context.PlcCodes on labcat.BillLabCatCd equals plc.LaborCategoryCode into gj
                from plc in gj.DefaultIfEmpty()
                where labcat.ProjId == ProjId
                select new PlcCodeDTO
                {
                    LaborCategoryCode = labcat.BillLabCatCd,
                    Description = plc.Description
                }).Distinct().ToList();
        }
        else
        {
            projects[0].Plc = (
                    from labcat in _context.PlcCodes
                    select new PlcCodeDTO
                    {
                        LaborCategoryCode = labcat.LaborCategoryCode,
                        Description = labcat.Description
                    }).Distinct().ToList();
        }


        return Task.FromResult((IEnumerable<ProjectDTO>)projects);

    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId)
    {
        //var test = _context.AccountGroupSetup.FirstOrDefault(p => p.AcctGroupCode.ToUpper() == "DIR" && p.AccountFunctionDescription.ToUpper() == "REVENUE");
        var validDescriptions = new List<string> { "REVENUE", "NON-LABOR", "LABOR", "UNALLOW-LABOR" };
        var projects = _context.PlProjects.Where(p => p.ProjId == ProjId).Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
            StartDate = h.ProjStartDt,
            EndDate = h.ProjEndDt,
            Description = h.Notes,
            OrgId = h.OrgId,
            Type = h.ProjTypeDc,
            proj_f_cst_amt = h.proj_f_cst_amt,
            proj_f_tot_amt = h.proj_f_tot_amt,
            proj_f_fee_amt = h.proj_f_fee_amt,
            AccountGroupCode = h.AcctGrpCd
            //accounts = _context.AccountGroupSetup.Where(p=>p.AcctGroupCode == h.AcctGrpCd && validDescriptions.Contains(p.AccountFunctionDescription.ToUpper())).Select(p=> new AccountGroupSetupDTO { AccountId = p.AccountId, AccountFunctionDescription = p.AccountFunctionDescription}).ToList()
        })
        .ToList();

        if (projects != null && projects.Count() > 0)
        {
            var orgs = _context.Organizations.Where(p => projects.Select(q => q.OrgId).Contains(p.OrgId)).ToList();
            foreach (var project in projects)
            {
                project.OrgName = orgs.FirstOrDefault(p => p.OrgId == project.OrgId)?.OrgName;
            }
        }

        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();

        var Allplcs = _context.PlcCodes.ToList();
        //var test = _context.AccountGroupSetup.Where(p => p.AcctGroupCode == projects[0].AccountGroupCode && validDescriptions.Contains(p.AccountFunctionDescription.ToUpper())).Select(p => new AccountGroupSetupDTO { AccountId = p.AccountId, AccountFunctionDescription = p.AccountFunctionDescription }).ToList();

        var test = (from ags in _context.AccountGroupSetup
                    join a in _context.Accounts
                        on ags.AccountId equals a.AcctId
                    where ags.AcctGroupCode == projects[0].AccountGroupCode
                       && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
                    select new AccountGroupSetupDTO
                    {
                        AccountId = ags.AccountId,
                        AccountFunctionDescription = ags.AccountFunctionDescription,
                        AcctName = a.AcctName
                    }).ToList();


        foreach (var account in test)
        {
            var acc = chartOfAccounts.Where(p => string.Join("-", p.AccountId.Split('-').Take(2)) == string.Join("-", account.AccountId.Split("-").Take(2))).FirstOrDefault();
            if (acc != null)
                account.BudgetSheet = acc.BudgetSheet;
        }

        projects[0].RevenueAccount = test.FirstOrDefault(p => p?.AccountFunctionDescription == "REVENUE")?.AccountId;
        projects[0].EmployeeLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "LABOR" && (p?.BudgetSheet?.ToUpper() == "Employee".ToUpper() || p?.BudgetSheet?.ToUpper() == "Staff Hours".ToUpper())).ToList();
        projects[0].EmployeeLaborAccounts.AddRange(test.Where(p => p?.AccountFunctionDescription == "UNALLOW-LABOR" && (p?.BudgetSheet?.ToUpper() == "Employee".ToUpper() || p?.BudgetSheet?.ToUpper() == "Staff Hours".ToUpper())).ToList());
        projects[0].EmployeeNonLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "NON-LABOR" && p?.BudgetSheet?.ToUpper() == "Employee".ToUpper()).ToList();
        projects[0].SunContractorLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "LABOR" && (p?.BudgetSheet?.ToUpper() == ("Subcontractor Hours".ToUpper()) || p?.BudgetSheet?.ToUpper() == "Subcontractors".ToUpper())).ToList();
        projects[0].SunContractorLaborAccounts.AddRange(test.Where(p => p?.AccountFunctionDescription == "UNALLOW-LABOR" && (p?.BudgetSheet?.ToUpper() == "Subcontractor Hours".ToUpper() || p?.BudgetSheet?.ToUpper() == "Subcontractors".ToUpper())).ToList());
        projects[0].SubContractorNonLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "NON-LABOR" && p?.BudgetSheet?.ToUpper() == "Subcontractor Hours".ToUpper()).ToList();
        projects[0].OtherDirectCostNonLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "NON-LABOR" && p?.BudgetSheet?.ToUpper() == "Other Direct Costs".ToUpper()).ToList();
        projects[0].OtherDirectCostLaborAccounts = test.Where(p => p?.AccountFunctionDescription == "LABOR" && p?.BudgetSheet?.ToUpper() == "Other Direct Costs".ToUpper()).ToList();

        string workforce = "false";
        try
        {
            workforce = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "workforce" && r.ProjId == ProjId)?.Value ?? "false";
        }
        catch (Exception ex) { }

        if (workforce.ToUpper() == "TRUE")
        {


            projects[0].Plc = (
                from labcat in _context.ProjEmployeeLabcats
                join plc in _context.PlcCodes on labcat.BillLabCatCd equals plc.LaborCategoryCode into gj
                from plc in gj.DefaultIfEmpty()
                where labcat.ProjId == ProjId
                select new PlcCodeDTO
                {
                    LaborCategoryCode = labcat.BillLabCatCd,
                    Description = plc.Description
                }).Distinct().ToList();
        }
        else
        {
            projects[0].Plc = (
                    from labcat in _context.ProjEmployeeLabcats
                    join plc in _context.PlcCodes on labcat.BillLabCatCd equals plc.LaborCategoryCode into gj
                    from plc in gj.DefaultIfEmpty()
                    select new PlcCodeDTO
                    {
                        LaborCategoryCode = labcat.BillLabCatCd,
                        Description = plc.Description
                    }).Distinct().ToList();
        }


        return Task.FromResult((IEnumerable<ProjectDTO>)projects);

    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectsByOrg(string orgId)
    {
        var projects = _context.PlProjects.Where(p => p.OrgId.Equals(orgId)).Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
            StartDate = h.ProjStartDt,
            EndDate = h.ProjEndDt,
            Description = h.Notes,
            OrgId = h.OrgId
        }).ToList();
        return Task.FromResult((IEnumerable<ProjectDTO>)projects);

    }

    //public async Task<List<PlConfigValue>> GetAllConfigValuesByProject(string projID)
    //{
    //    return await _context.PlConfigValues.Where(p => p.ProjId == projID).ToListAsync();
    //}


    //public async Task<PlConfigValue> GetConfigValueByName(string name)
    //{
    //    var config = await _context.PlConfigValues
    //        .FirstOrDefaultAsync(p => p.Name == name);

    //    if (config == null)
    //    {
    //        return null;
    //    }

    //    return config;
    //}

    //public async Task<PlConfigValue> AddConfigValue(PlConfigValue plConfigValue)
    //{
    //    _context.PlConfigValues.Add(plConfigValue);
    //    await _context.SaveChangesAsync();
    //    return plConfigValue;
    //}


    //public async Task<bool> DeleteConfigValueByName(string name)
    //{
    //    var config = await _context.PlConfigValues.FindAsync(name);
    //    if (config == null)
    //        return false;

    //    _context.PlConfigValues.Remove(config);
    //    await _context.SaveChangesAsync();
    //    return true;
    //}

    //public async Task<bool> DeleteConfigValueById(int id)
    //{
    //    var config = await _context.PlConfigValues.FirstOrDefaultAsync(p => p.Id == id);
    //    if (config == null)
    //        return false;

    //    _context.PlConfigValues.Remove(config);
    //    await _context.SaveChangesAsync();
    //    return true;
    //}
    //public async Task<bool> UpdateConfigValue(PlConfigValue plConfigValue)
    //{
    //    var existing = await _context.PlConfigValues.FirstOrDefaultAsync(p => p.Name == plConfigValue.Name && p.ProjId == plConfigValue.ProjId);
    //    if (existing == null)
    //        return false;

    //    // Update fields
    //    existing.Value = plConfigValue.Value;
    //    _context.PlConfigValues.Update(existing);
    //    await _context.SaveChangesAsync();
    //    return true;
    //}

    public Task<List<PlcCode>> GetAllPlcs(string? plc)
    {

        if (string.IsNullOrEmpty(plc))
            return _context.PlcCodes
                    .Select(p => new PlcCode
                    {
                        LaborCategoryCode = p.LaborCategoryCode,
                        Description = p.Description
                    })
                    .ToListAsync();
        else
            return _context.PlcCodes.Where(p => p.LaborCategoryCode.ToLower().StartsWith(plc.ToLower()))
                .Select(p => new PlcCode
                {
                    LaborCategoryCode = p.LaborCategoryCode,
                    Description = p.Description
                })
                .ToListAsync();
    }

    public Task<List<PlcCode>> GetFundingDetails(string plc)
    {
        return _context.PlcCodes.Where(p => p.LaborCategoryCode.ToLower().StartsWith(plc.ToLower()))
            .Select(p => new PlcCode
            {
                LaborCategoryCode = p.LaborCategoryCode,
                Description = p.Description
            })
            .ToListAsync();
    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForValidate(string projId)
    {
        var projects = _context.PlProjectPlans.Where(p => p.ProjId.StartsWith(projId)).Select(h => new ProjectDTO
        {
            ProjectId = h.ProjId,
            Name = h.ProjName,
        })
            .ToList();

        return Task.FromResult((IEnumerable<ProjectDTO>)projects);
    }
}