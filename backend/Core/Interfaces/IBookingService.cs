using AutoServ.Core.Entities;
using AutoServ.Core.Models;
using AutoServ.DTOs.Admin;
using AutoServ.DTOs.Bookings;

namespace AutoServ.Core.Interfaces
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<Result<List<BookingHistoryDto>>> GetUserHistoryAsync(int userId);
        Task<Result<List<MasterScheduleTaskDto>>> GetMasterScheduleAsync(int userId, DateTime date);
        Task<Result<BookingCreatedResultDto>> CreateAsync(CreateBookingDto dto);
        Task<Result> UpdateTaskStatusAsync(int taskId, string newStatus);


        Task<List<AdminBookingDto>> GetAdminTableAsync(string? dateFilter = null);

        Task<List<AdminScheduleTaskDto>> GetAdminFullScheduleAsync(string? dateFilter = null);
    }
}