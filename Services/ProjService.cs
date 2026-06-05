namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface IProjService
{
    Task<IEnumerable<ProjectDTO>> GetAllProjects();
    Task<IEnumerable<ProjectDTO>> GetAllProjects(int UserId);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(string proj_id);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(int UserId, string Role, string proj_id);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsForValidate(string proj_id);
    Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId);
    Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId, string PlType);
    Task<IEnumerable<ProjectDTO>> GetAllProjectsByOrg(string orgId);
    Task<List<PlcCode>> GetAllPlcs(string? plc);
}

public class ProjService : IProjService
{
    private IProjRepository _projRepository;

    public ProjService(IProjRepository projRepository)
    {
        _projRepository = projRepository;
    }

    public Task<List<PlcCode>> GetAllPlcs(string? plc)
    {
        return _projRepository.GetAllPlcs(plc);
    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId)
    {
        return _projRepository.GetAllProjectByProjId(ProjId);
    }
    public Task<IEnumerable<ProjectDTO>> GetAllProjectByProjId(string ProjId, string PlType)
    {
        return _projRepository.GetAllProjectByProjId(ProjId, PlType);
    }
    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(string ProjId)
    {
        return _projRepository.GetAllProjectsForSearch(ProjId);
    }

    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForSearch(int UserId, string Role, string proj_id)
    {
        return _projRepository.GetAllProjectsForSearch(UserId, Role, proj_id);
    }
    public Task<IEnumerable<ProjectDTO>> GetAllProjectsForValidate(string ProjId)
    {
        return _projRepository.GetAllProjectsForValidate(ProjId);
    }
    public async Task<IEnumerable<ProjectDTO>> GetAllProjects()
    {
        return await _projRepository.GetAllProjects();
    }
    public async Task<IEnumerable<ProjectDTO>> GetAllProjects(int UserId)
    {
        return await _projRepository.GetAllProjects(UserId);
    }

    public async Task<IEnumerable<ProjectDTO>> GetAllProjectsByOrg(string orgId)
    {
        return await _projRepository.GetAllProjectsByOrg(orgId);
    }

}