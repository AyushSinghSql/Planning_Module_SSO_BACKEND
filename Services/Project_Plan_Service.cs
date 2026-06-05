namespace WebApi.Services;

using System.Numerics;
using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface IProjPlanService
{
    Task<IEnumerable<PlProjectPlan>> GetProjectPlans(string projectID);
    //Task<PagedResult<PlProjectPlan>> GetProjectPlans(int userId, string role, string? projectId, int pageNumber, int pageSize);
    Task<IEnumerable<PlProjectPlan>> GetProjectPlans(int UserId,string Role, string projectID, string? status, string fetchNewBussiness);
    Task<IEnumerable<PlProjectPlan>> GetProjectPlansV1(int UserId, string Role, string projectID, string? status, string fetchNewBussiness);
    Task<PagedResponse<PlProjectPlan>> GetProjectPlansPaged(int userId, string role, string projectID, string? status, string? planstatus, string? planType, string fetchNewBussiness, int pageNumber, int pageSize);
    Task<IEnumerable<forecast>> GetForecastByPlanID(int planID);
    Task<PlProjectPlan> AddProjectPlanAsync(PlProjectPlan newPlan, string type);
    Task<PlProjectPlan> AddProjectPlanForImportAsync(PlProjectPlan newPlan, string type);
    Task<PlProjectPlan> AddPBudgetFromNewBussinessAsync(NewBusinessBudgetDTO newPlan, string SourceProject);
    Task<bool> UpdateProjectPlanAsync(PlProjectPlan updatedPlan);
    Task<bool> BulkUpdateProjectPlansAsync(List<PlProjectPlan> updatedPlans);
    Task<bool> DeleteProjectPlanAsync(int planId);
    Task<IEnumerable<forecast>> GetEACDataByPlanId(int planId);
    Task<IEnumerable<Account>> GetAccountsByProjectId(string projId);
    Task<IEnumerable<PlEmployee>> GetEmployeesByProjectId(string projId);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int year);
    Task<IEnumerable<PlProjectPlan>> GetAllNewBussiness(string nbId);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostEACDataByPlanId(int planId, int year);
    Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int year);
    Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize);
}

public class ProjPlanService : IProjPlanService
{
    private IProjPlanRepository _projPlanRepository;

    public ProjPlanService(IProjPlanRepository projPlanRepository)
    {
        _projPlanRepository = projPlanRepository;
    }

    public async Task<IEnumerable<PlProjectPlan>> GetProjectPlans(string projectID)
    {
        return await _projPlanRepository.GetProjectPlans(projectID);
    }

    public async Task<IEnumerable<PlProjectPlan>> GetProjectPlans(int UserId, string Role, string projectID, string? status, string fetchNewBussiness)
    {
        return await _projPlanRepository.GetProjectPlans(UserId, Role, projectID, status, fetchNewBussiness);
    }
    public async Task<IEnumerable<PlProjectPlan>> GetProjectPlansV1(int UserId, string Role, string projectID, string? status, string type)
    {
        return await _projPlanRepository.GetProjectPlansV1(UserId, Role, projectID, status, type);
    }
    public async Task<PagedResponse<PlProjectPlan>> GetProjectPlansPaged(int userId, string role, string projectID, string? status, string? planstatus, string? planType, string fetchNewBussiness, int pageNumber, int pageSize)
    {
        return await _projPlanRepository.GetProjectPlansPaged(userId, role, projectID, status, planstatus, planType, fetchNewBussiness,pageNumber,pageSize);
    }
    //public async Task<PagedResult<PlProjectPlan>> GetProjectPlans(int userId, string role, string? projectId, int pageNumber, int pageSize)
    //{
    //    return await _projPlanRepository.GetProjectPlans(userId, role, projectId, pageNumber, pageSize);
    //}
    public async Task<IEnumerable<forecast>> GetForecastByPlanID(int planID)
    {
        return await _projPlanRepository.GetForecastByPlanID(planID);
    }
    public async Task<PlProjectPlan> AddPBudgetFromNewBussinessAsync(NewBusinessBudgetDTO newBusinessBudgetnewPlan, string SourceProject)
    {

        return await _projPlanRepository.AddPBudgetFromNewBussinessAsync(newBusinessBudgetnewPlan, SourceProject);
    }

    public async Task<PlProjectPlan> AddProjectPlanAsync(PlProjectPlan newPlan, string type)
    {
        try
        {
            return await _projPlanRepository.AddProjectPlanAsync(newPlan, type);
        }
        catch (Exception ex)
        {
            // Optional: log the exception
            // _logger.LogError(ex, "Error occurred while adding a project plan.");

            // You can rethrow, wrap, or handle depending on your needs
            throw new ApplicationException(ex.Message, ex);
        }
    }

    public async Task<PlProjectPlan> AddProjectPlanForImportAsync(PlProjectPlan newPlan, string type)
    {
        try
        {
            return await _projPlanRepository.AddProjectPlanAsync(newPlan, type);
        }
        catch (Exception ex)
        {
            // Optional: log the exception
            // _logger.LogError(ex, "Error occurred while adding a project plan.");

            // You can rethrow, wrap, or handle depending on your needs
            throw new ApplicationException("An error occurred while adding the project plan.", ex);
        }
    }

    public async Task<bool> UpdateProjectPlanAsync(PlProjectPlan updatedPlan)
    {
        return await _projPlanRepository.UpdateProjectPlanAsync(updatedPlan);
    }

    public async Task<bool> BulkUpdateProjectPlansAsync(List<PlProjectPlan> updatedPlans)
    {
        return await _projPlanRepository.BulkUpdateProjectPlansAsync(updatedPlans);
    }
    public async Task<bool> DeleteProjectPlanAsync(int planId)
    {
        return await _projPlanRepository.DeleteProjectPlanAsync(planId);
    }

    public Task<IEnumerable<forecast>> GetEACDataByPlanId(int planId)
    {
        return _projPlanRepository.GetEACDataByPlanId(planId);
    }

    public Task<IEnumerable<Account>> GetAccountsByProjectId(string projId)
    {
        return _projPlanRepository.GetAccountsByProjectId(projId);
    }

    public Task<IEnumerable<PlEmployee>> GetEmployeesByProjectId(string projId)
    {
        return _projPlanRepository.GetEmployeesByProjectId(projId);
    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int year)
    {
        return _projPlanRepository.GetDirectCostForecastByPlanID(planID, year);
    }

    public Task<IEnumerable<PlProjectPlan>> GetAllNewBussiness(string nbId)
    {
        return _projPlanRepository.GetAllNewBussiness(nbId);
    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostEACDataByPlanId(int planId, int year)
    {
        return _projPlanRepository.GetDirectCostEACDataByPlanId(planId,year);
    }

    public Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int year)
    {
        return _projPlanRepository.GetEmployeeForecastByPlanID(planID, year);
    }

    public Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int? year, string? emplid,
    int pageNumber,
    int pageSize)
    {
        return _projPlanRepository.GetEmployeeForecastByPlanID(planID, year, emplid, pageNumber, pageSize);
    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize)
    {
        return _projPlanRepository.GetDirectCostForecastByPlanID(planID, year, emplid, pageNumber, pageSize);
    }
}