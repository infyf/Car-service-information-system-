using AutoServ.Core.Entities;
using AutoServ.DTOs.Bookings;

namespace AutoServ.Core.Interfaces
{
    public interface IMasterAllocationStrategy
    {
        Task<Master?> FindMasterAsync(
            int serviceId,
            DateTime date,
            TimeSpan requiredTime,
            int durationMinutes,
            List<(Master Master, List<BookingServiceItemDto> Services, TimeSpan StartTime)> currentPlan = null);
    }
}