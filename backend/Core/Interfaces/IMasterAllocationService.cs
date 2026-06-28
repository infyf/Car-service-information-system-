using AutoServ.DTOs.Bookings;

namespace AutoServ.Core.Interfaces
{
    public interface IMasterAllocationService
    {
        Task<List<(int? MasterId, List<BookingServiceItemDto> Services)>> AllocateMastersAsync(List<BookingServiceItemDto> services, DateTime date);
    }
}