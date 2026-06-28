using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Bookings;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class MasterAllocationService : IMasterAllocationService
    {
        private readonly ApplicationContext _context;

        public MasterAllocationService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<(int? MasterId, List<BookingServiceItemDto> Services)>> AllocateMastersAsync(List<BookingServiceItemDto> services, DateTime date)
        {
            var bookingGroups = new List<(int? MasterId, List<BookingServiceItemDto> Services)>();

            foreach (var service in services)
            {
                int? masterId = await FindBestMasterForServiceAsync(service.Id, date);

                var existingGroupIndex = bookingGroups.FindIndex(g => g.MasterId == masterId);

                if (existingGroupIndex != -1)
                {
                    var group = bookingGroups[existingGroupIndex];
                    group.Services.Add(service);
                    bookingGroups[existingGroupIndex] = group;
                }
                else
                {
                    bookingGroups.Add((masterId, new List<BookingServiceItemDto> { service }));
                }
            }

            return bookingGroups;
        }

        private async Task<int?> FindBestMasterForServiceAsync(int serviceItemId, DateTime date)
        {
            var candidateMasters = await _context.Masters
                .Where(m => m.IsActive)
                .Where(m => m.ServiceItems.Any(s => s.Id == serviceItemId))
                .ToListAsync();

            if (!candidateMasters.Any()) return null;

            int? bestMasterId = null;
            int minLoad = int.MaxValue;

            foreach (var master in candidateMasters)
            {
                var load = await _context.Bookings
                    .CountAsync(b => b.MasterId == master.Id &&
                                     b.BookingDate.Date == date.Date &&
                                     b.Status != BookingStatus.Cancelled && 
                                     b.Status != BookingStatus.Completed); 

                if (load < minLoad)
                {
                    minLoad = load;
                    bestMasterId = master.Id;
                }
            }

            return bestMasterId;
        }
    }
}