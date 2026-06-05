namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface IOrgService
{
    Task<IEnumerable<Organization>> GetAllOrgs();
    Task<IEnumerable<Holidaycalender>> GetOrgHolidays();
    Task<IEnumerable<OrgAccount>> GetAllOrgAccounts(string OrgID);
    Task<IEnumerable<BurdenTemplate>> GetAllTemplates();
    Task<IEnumerable<OrgAccount>> GetAllOrgAccounts();
}

public class OrgService : IOrgService
{
    private IOrgRepository _orgRepository;

    public OrgService(
        IOrgRepository orgRepository,
        IMapper mapper)
    {
        _orgRepository = orgRepository;
    }


    public async Task<IEnumerable<Organization>> GetAllOrgs()
    {
        return await _orgRepository.GetAllOrgs();
    }
    public async Task<IEnumerable<Holidaycalender>> GetOrgHolidays()
    {
        return await _orgRepository.GetOrgHolidays();
    }

    public async Task<IEnumerable<OrgAccount>> GetAllOrgAccounts(string OrgID)
    {
        return await _orgRepository.GetAllOrgAccounts(OrgID);

    }
    public async Task<IEnumerable<OrgAccount>> GetAllOrgAccounts()
    {
        return await _orgRepository.GetAllOrgAccounts();

    }

    public async Task<IEnumerable<BurdenTemplate>> GetAllTemplates()
    {
        return await _orgRepository.GetAllTemplates();

    }
}