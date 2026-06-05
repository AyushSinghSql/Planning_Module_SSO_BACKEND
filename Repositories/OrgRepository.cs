namespace WebApi.Repositories;

using PlanningAPI.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

public interface IOrgRepository
{
    Task<IEnumerable<Organization>> GetAllOrgs();
    Task<IEnumerable<Holidaycalender>> GetOrgHolidays();
    Task<IEnumerable<OrgAccount>> GetAllOrgAccounts(string OrgID);
    Task<IEnumerable<BurdenTemplate>> GetAllTemplates();
    Task<IEnumerable<OrgAccount>> GetAllOrgAccounts();
}

public class OrgRepository : IOrgRepository
{
    //private DataContext _context;
    private readonly MydatabaseContext _context;

    public OrgRepository(MydatabaseContext context)
    {
        _context = context;
    }
    public Task<IEnumerable<Organization>> GetAllOrgs()
    {
        var orgs = _context.Organizations.ToList<Organization>();
        return Task.FromResult((IEnumerable<Organization>)orgs);

    }
    public Task<IEnumerable<Holidaycalender>> GetOrgHolidays()
    {
        var HolidayList = _context.Holidaycalenders.ToList<Holidaycalender>();
        return Task.FromResult((IEnumerable<Holidaycalender>)HolidayList);

    }

    public Task<IEnumerable<OrgAccount>> GetAllOrgAccounts(string OrgID)
    {
        var orgAccounts = _context.OrgAccounts.Where(p=>p.OrgId == OrgID).ToList<OrgAccount>();
        return Task.FromResult((IEnumerable<OrgAccount>)orgAccounts);

    }

    public Task<IEnumerable<OrgAccount>> GetAllOrgAccounts()
    {
        var orgAccounts = _context.OrgAccounts.ToList<OrgAccount>();
        return Task.FromResult((IEnumerable<OrgAccount>)orgAccounts);

    }

    public async Task<IEnumerable<BurdenTemplate>> GetAllTemplates() // ✅ correct
    {
        var templates = await _context.BurdenTemplates.ToListAsync();
        return templates;
    }

}