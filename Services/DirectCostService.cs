namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface IDirectCostService
{
    Task<List<PlDct>> GetAllDirectCosts();
    //Task<List<PlDct>> GetAllDirectCostsByProject(string projId);

    Task<PlDct> AddNewDirectCostAsync(PlDct plDct, string projId);
    Task AddNewDirectCostsAsync(List<PlDct> plDct, string projId);
    Task<bool> UpdateDirectCostAsync(PlDct updatedDirectCost);
    Task<PlDct> GetDirectCostByID(int plDctId);
    Task<List<PlDct>> GetAllDirectCostsByPlanId(int planId);
    Task<bool> DeleteDirectCostAsync(int id);
}

public class DirectCostService : IDirectCostService
{
    private IDirectCostRepository _directCostRepository;

    public DirectCostService(IDirectCostRepository directCostRepository, IMapper mapper)
    {
        _directCostRepository = directCostRepository;
    }

    public async Task<PlDct> AddNewDirectCostAsync(PlDct newPlDct, string projId)
    {
        return await _directCostRepository.AddNewDirectCostAsync(newPlDct, projId);
    }
    //public void AddNewDirectCostsAsync(List<PlDct> newPlDct, string projId)
    //{
    //    _directCostRepository.AddNewDirectCostsAsync(newPlDct, projId);
    //}
    public async Task<List<PlDct>> GetAllDirectCosts()
    {
        return await _directCostRepository.GetAllDirectCosts();
    }

    public async Task<List<PlDct>> GetAllDirectCostsByPlanId(int planId)
    {
        return await _directCostRepository.GetAllDirectCostsByPlanId(planId);
    }

    public async Task<PlDct> GetDirectCostByID(int dctId)
    {
        return await _directCostRepository.GetDirectCostByID(dctId);
    }

    public async Task<bool> UpdateDirectCostAsync(PlDct updatedDirectCost)
    {
        return await _directCostRepository.UpdateDirectCostAsync(updatedDirectCost);

    }
    public async Task<bool> DeleteDirectCostAsync(int id)
    {
        return await _directCostRepository.DeleteDirectCostAsync(id);

    }

    public async Task AddNewDirectCostsAsync(List<PlDct> plDct, string projId)
    {
        await _directCostRepository.AddNewDirectCostsAsync(plDct, projId);
    }
}