using AutoServ.Core.Models;
using AutoServ.DTOs.Masters;

namespace AutoServ.Core.Interfaces
{
    public interface IMasterService
    {
        Task<List<MasterDto>> GetAllActiveAsync();
        Task<object?> GetByIdAsync(int id);
        Task<Result> UpdateSpecializationsAsync(int id, UpdateMasterSpecializationsDto dto);
        Task<Result> AddSpecializationAsync(int id, int serviceItemId);
    }
}