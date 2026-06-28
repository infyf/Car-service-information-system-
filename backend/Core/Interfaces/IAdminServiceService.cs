using AutoServ.Core.Models;
using AutoServ.DTOs.Admin;

namespace AutoServ.Core.Interfaces
{
    public interface IAdminServiceService
    {


        Task<AdminDashboardStatsDto> GetDashboardStatsAsync(string? date = null);
        Task<List<AdminClientDto>> GetClientsAsync();
        Task<List<AdminMasterScheduleDto>> GetMastersScheduleAsync(string? date = null);
        Task<List<AdminMasterDetailedDto>> GetMastersDetailedAsync();
        Task<Result> ToggleMasterStatusAsync(int id);
        Task<Result> RemoveSpecializationAsync(int masterId, int serviceItemId);

 

        Task<Result<object>> CreateServiceAsync(ManageServiceDto dto);
        Task<Result<object>> UpdateServiceAsync(int serviceId, ManageServiceDto dto);
        Task<Result> DeleteServiceAsync(int serviceId);

        Task<Result<object>> AddItemAsync(int serviceId, ManageServiceItemDto dto);
        Task<Result<object>> UpdateItemAsync(int itemId, ManageServiceItemDto dto);
        Task<Result> DeleteItemAsync(int itemId);

        Task<List<AdminServiceListDto>> GetAllForAdminAsync();
        Task<Result> UpdateServiceDescriptionAsync(int serviceId, string description);
        Task<Result> UpdateServiceTitleAsync(int serviceId, string title);
    }
}