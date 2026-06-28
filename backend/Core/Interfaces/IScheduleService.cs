using AutoServ.DTOs.Bookings;

namespace AutoServ.Core.Interfaces
{
    public interface IScheduleService
    {
        Task<MasterScheduleDto> GetMasterScheduleAsync(int masterId, DateTime date);
        Task<bool> IsTimeSlotAvailableAsync(int masterId, DateTime date, TimeSpan startTime, int durationMinutes);
        Task<TimeSpan?> FindFirstAvailableSlotAsync(int masterId, DateTime date, int durationMinutes, TimeSpan? searchFromTime = null);
    }
}