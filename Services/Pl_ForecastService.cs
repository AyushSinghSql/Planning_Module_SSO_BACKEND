namespace WebApi.Services;

using System;
using System.Collections.Generic;
using System.Numerics;
using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;
using YourNamespace.Models;

public interface IPl_ForecastService
{
    Task AddAsync(PlForecast forecast);
    Task AddRangeAsync(List<PlForecast> forecasts);
    Task UpdateAsync(PlForecast forecast);
    Task DeleteAsync(int forecastId);
    Task<PlForecast?> GetByIdAsync(int forecastId);
    Task<List<PlForecast>> GetAllAsync();
    Task UpdateAmountAsync(PlForecast forecast);
    Task UpdateHoursAsync(PlForecast forecast);
    Task UpdateAmountAsync(PlForecast forecast, string type);
    Task UpdateHoursAsync(PlForecast forecast, string type);
    Task<PlanForecastSummary> CalculateCost(int planID, int templateId, string type);
    Task<PlanForecastSummary> CalculateCostAI(int planID, int templateId, string type, int Year);
    Task<List<PlForecast>> GetByPlanIdAsync(int planId);
    Task<List<PlForecast>> GetForecastByProjectIDAndVersion(string projId, int version, string type);
    Task<List<PlForecast>> GetByPlanAndProjectAsync(int planId, string projectId);
    Task<PlanForecastSummary> CalculateRevenueCost(int planID, int templateId, string type);
    Task UpdateAmountAsync(List<PlForecast> forecast, string type);
    Task UpdateHoursAsync(List<PlForecast> forecasts, string type);
    Task UpsertAmountAsync(List<PlForecast> forecasts, int plid, int templateId, string type);
    Task UpsertHoursAsync(List<PlForecast> forecasts, int plid, int templateId, string type);
    //void CalculateBurdenCost(int planID, int templateId, string type);
    Task<PlanForecastSummary> CalculateRevenueCostForSelectedHours(int planID, int templateId, string type, List<PlForecast> hoursForecast);
    Task<PlanForecastSummary> CalculateBurdenCost(int planID, int templateId, string type);
}

public class Pl_ForecastService : IPl_ForecastService
{
    private IPlForecastRepository _pl_ForecastRepository;

    public Pl_ForecastService(IPlForecastRepository pl_ForecastRepository, IMapper mapper)
    {
        _pl_ForecastRepository = pl_ForecastRepository;
    }
    public Task AddAsync(PlForecast forecast)
    {
        return _pl_ForecastRepository.AddAsync(forecast);
    }
    public Task AddRangeAsync(List<PlForecast> forecasts)
    {
        return _pl_ForecastRepository.AddRangeAsync(forecasts);
    }
    public Task DeleteAsync(int forecastId)
    {
        return _pl_ForecastRepository.DeleteAsync(forecastId);

    }

    public Task<List<PlForecast>> GetAllAsync()
    {
        return _pl_ForecastRepository.GetAllAsync();
    }

    public Task<PlForecast?> GetByIdAsync(int forecastId)
    {
        return _pl_ForecastRepository.GetByIdAsync(forecastId);
    }
    public Task<List<PlForecast>> GetByPlanIdAsync(int planId)
    {
        return _pl_ForecastRepository.GetByPlanIdAsync(planId);
    }

    public Task<List<PlForecast>> GetByPlanAndProjectAsync(int planId, string projectId)
    {
        return _pl_ForecastRepository.GetByPlanIdandProjectIdAsync(planId, projectId);
    }
    public Task UpdateAsync(PlForecast forecast)
    {
        return _pl_ForecastRepository.UpdateAsync(forecast);
    }

    public Task UpdateAmountAsync(PlForecast forecast)
    {
        return _pl_ForecastRepository.UpdateAmountAsync(forecast);

    }
    public Task UpdateHoursAsync(PlForecast forecast)
    {
        return _pl_ForecastRepository.UpdateHoursAsync(forecast);

    }

    public Task UpdateAmountAsync(PlForecast forecast, string type)
    {
        return _pl_ForecastRepository.UpdateAmountAsync(forecast, type);

    }
    public Task UpdateAmountAsync(List<PlForecast> forecast, string type)
    {
        return _pl_ForecastRepository.UpsertAmountAsync(forecast, type);

    }
    public Task UpdateHoursAsync(List<PlForecast> forecast, string type)
    {
        return _pl_ForecastRepository.UpsertHoursAsync(forecast, type);

    }

    public Task UpsertAmountAsync(List<PlForecast> forecast, int plid, int templateid, string type)
    {
        return _pl_ForecastRepository.UpsertAmountAsync(forecast,plid,templateid, type);

    }
    public Task UpsertHoursAsync(List<PlForecast> forecast, int plid, int templateid, string type)
    {
        return _pl_ForecastRepository.UpsertHoursAsync(forecast, plid, templateid, type);

    }
    public Task UpdateHoursAsync(PlForecast forecast, string type)
    {
        return _pl_ForecastRepository.UpdateHoursAsync(forecast, type);

    }
    public Task<PlanForecastSummary> CalculateCost(int planID, int templateId, string type)
    {
        return _pl_ForecastRepository.CalculateCost(planID, templateId, type);

    }

    public Task<PlanForecastSummary> CalculateCostAI(int planID, int templateId, string type, int Year)
    {
        return _pl_ForecastRepository.CalculateCostAI(planID, templateId, type, Year);

    }

    //public void CalculateBurdenCost(int planID, int templateId, string type)
    //{
    //    _pl_ForecastRepository.CalculateBurdenCost(planID, templateId, type);
    //}

    public Task<List<PlForecast>> GetForecastByProjectIDAndVersion(string projId, int version, string type)
    {
        return _pl_ForecastRepository.GetForecastByProjectIDAndVersion(projId, version, type);
    }
    public Task<PlanForecastSummary> CalculateRevenueCost(int planID, int templateId, string type)
    {
        return _pl_ForecastRepository.CalculateRevenueCost(planID, templateId, type);

    }
    public Task<PlanForecastSummary> CalculateRevenueCostForSelectedHours(int planID, int templateId, string type, List<PlForecast> hoursForecast)
    {
        return _pl_ForecastRepository.CalculateRevenueCostForSelectedHours(planID, templateId, type, hoursForecast);

    }

    public Task<PlanForecastSummary> CalculateBurdenCost(int planID, int templateId, string type)
    {
        return _pl_ForecastRepository.CalculateBurdenCost(planID, templateId, type);

    }
}