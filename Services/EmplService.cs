namespace WebApi.Services;

using AutoMapper;
using BCrypt.Net;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface IEmplService
{
    Task<List<PlEmployeee>> GetAllEmployees();
    //Task<List<PlEmployee>> GetAllEployeesByProject(string projId);
    Task<PlEmployeee> AddNewEmployeeAsync(PlEmployeee newPlEmployee);
    Task AddNewEmployeesAsync(List<PlEmployeee> newPlEmployee);
    Task<bool> UpdateEmployeeAsync(PlEmployeee updatedEmployee);
    Task<PlEmployeee> GetEployeeByID(string emplId);
    Task<bool> DeleteEmployeeAsync(int employeeId);
}

public class EmplService : IEmplService
{
    private IEmplRepository _emplRepository;

    public EmplService(
        IEmplRepository emplRepository,
        IMapper mapper)
    {
        _emplRepository = emplRepository;
    }

    public async Task<PlEmployeee> AddNewEmployeeAsync(PlEmployeee newPlEmployee)
    {
        return await _emplRepository.AddNewEmployeeAsync(newPlEmployee);
    }
    public async Task AddNewEmployeesAsync(List<PlEmployeee> newPlEmployee)
    {
        await _emplRepository.AddNewEmployeesAsync(newPlEmployee);
    }

    public async Task<List<PlEmployeee>> GetAllEmployees()
    {
        return await _emplRepository.GetAllEployees();
    }

    //public async Task<List<PlEmployeee>> GetAllEployeesByProject(string projId)
    //{
    //    return (List<PlEmployee>)await _emplRepository.GetAllEployeesByProject(projId);
    //}

    public async Task<PlEmployeee> GetEployeeByID(string emplId)
    {
        return await _emplRepository.GetEployeeByID(emplId);
    }

    public async Task<bool> DeleteEmployeeAsync(int emplId)
    {
        return await _emplRepository.DeleteEmployeeAsync(emplId);
    }

    public async Task<bool> UpdateEmployeeAsync(PlEmployeee updatedEmployee)
    {
        return await _emplRepository.UpdateEmployeeAsync(updatedEmployee);

    }
}