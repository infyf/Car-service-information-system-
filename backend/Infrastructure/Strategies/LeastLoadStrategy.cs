using AutoServ.Core.Entities;
using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Bookings;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Strategies
{
    public class LeastLoadStrategy : IMasterAllocationStrategy
    {
        private readonly ApplicationContext _context;
        private readonly IScheduleService _scheduleService;

        public LeastLoadStrategy(ApplicationContext context, IScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        public async Task<Master?> FindMasterAsync(
            int serviceId, DateTime date, TimeSpan requiredTime, int durationMinutes,
            List<(Master Master, List<BookingServiceItemDto> Services, TimeSpan StartTime)> currentPlan = null)
        {
            var candidates = await _context.Masters
                .Include(m => m.ServiceItems)
                .Include(m => m.Bookings)
                .Where(m => m.IsActive && m.ServiceItems.Any(s => s.Id == serviceId))
                .ToListAsync();

            Master? bestMaster = null;
            int lowestLoadScore = int.MaxValue;

            foreach (var m in candidates)
            {
                if (!await _scheduleService.IsTimeSlotAvailableAsync(m.Id, date, requiredTime, durationMinutes))
                    continue;

                int dbLoad = m.Bookings!.Count(b => b.BookingDate.Date == date.Date);
                int localLoad = currentPlan?.Count(p => p.Master.Id == m.Id) ?? 0;
                int totalLoad = dbLoad + localLoad;

                if (totalLoad < lowestLoadScore)
                {
                    lowestLoadScore = totalLoad;
                    bestMaster = m;
                }
            }

            return bestMaster;
        }
    }
}