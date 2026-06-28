using AutoServ.Core.Models;      
using AutoServ.DTOs.Admin;        
using AutoServ.DTOs.Services;

namespace AutoServ.Core.Interfaces
{
    public interface IServiceService
    {
        Task<List<object>> GetAllAsync();
        Task<ServiceDetailDto?> GetDetailAsync(int id);
        Task<List<object>> GetWithItemsAsync();


        Task<Result> CreateAsync(ManageServiceDto dto);
        Task<Result> UpdateAsync(int id, ManageServiceDto dto);
        Task<Result> DeleteAsync(int id);
    }
}