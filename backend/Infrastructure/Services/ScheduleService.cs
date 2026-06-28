using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Bookings;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationContext _context;

        private const int SLOT_DURATION_MINUTES = 30;
        private static readonly TimeSpan WORK_DAY_START = new TimeSpan(9, 0, 0);
        private static readonly TimeSpan WORK_DAY_END = new TimeSpan(18, 0, 0);
        private const int CONSULTATION_DURATION = 30;

        public ScheduleService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<MasterScheduleDto> GetMasterScheduleAsync(int masterId, DateTime date)
        {
            var master = await _context.Masters.FindAsync(masterId);
            if (master == null) throw new Exception("Майстра не знайдено");

            var bookings = await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Where(b => b.MasterId == masterId && b.BookingDate.Date == date.Date)
                .ToListAsync();

            var slots = new List<TimeSlotDto>();
            var currentTime = WORK_DAY_START;

            while (currentTime < WORK_DAY_END)
            {
                bool isBooked = false;
                int? bookingId = null;

                foreach (var b in bookings)
                {
                    int duration = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 0);
                    if (duration == 0) duration = CONSULTATION_DURATION;

                    var bStart = b.BookingTime;
                    var bEnd = bStart.Add(TimeSpan.FromMinutes(duration));

                    if (currentTime >= bStart && currentTime < bEnd)
                    {
                        isBooked = true;
                        bookingId = b.Id;
                        break;
                    }
                }

                slots.Add(new TimeSlotDto
                {
                    Time = currentTime.ToString(@"hh\:mm"),
                    IsBooked = isBooked,
                    BookingId = bookingId
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(SLOT_DURATION_MINUTES));
            }

            return new MasterScheduleDto
            {
                MasterId = masterId,
                MasterName = $"{master.FirstName} {master.LastName}",
                Date = date,
                Slots = slots
            };
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int masterId, DateTime date, TimeSpan startTime, int durationMinutes)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

            if (startTime < WORK_DAY_START || endTime > WORK_DAY_END) return false;

            var bookings = await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Where(b => b.MasterId == masterId &&
                            b.BookingDate.Date == date.Date &&
                            b.Status != BookingStatus.Cancelled)
                .ToListAsync();

            foreach (var b in bookings)
            {
                int bDuration = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 0);
                if (bDuration == 0) bDuration = CONSULTATION_DURATION;

                var bStart = b.BookingTime;
                var bEnd = bStart.Add(TimeSpan.FromMinutes(bDuration));

                if (startTime < bEnd && endTime > bStart)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<TimeSpan?> FindFirstAvailableSlotAsync(int masterId, DateTime date, int durationMinutes, TimeSpan? searchFromTime = null)
        {
            var currentTime = searchFromTime ?? WORK_DAY_START;
            if (currentTime < WORK_DAY_START) currentTime = WORK_DAY_START;

            var bookings = await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Where(b => b.MasterId == masterId && b.BookingDate.Date == date.Date && b.Status != BookingStatus.Cancelled) // ✅ ВИПРАВЛЕНО
                .ToListAsync();

            var intervals = bookings.Select(b => {
                int d = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 0);
                if (d == 0) d = CONSULTATION_DURATION;
                return new { Start = b.BookingTime, End = b.BookingTime.Add(TimeSpan.FromMinutes(d)) };
            }).ToList();

            while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= WORK_DAY_END)
            {
                var slotStart = currentTime;
                var slotEnd = slotStart.Add(TimeSpan.FromMinutes(durationMinutes));

                bool isAvailable = true;

                foreach (var interval in intervals)
                {
                    if (slotStart < interval.End && slotEnd > interval.Start)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    return currentTime;
                }

                currentTime = currentTime.Add(TimeSpan.FromMinutes(SLOT_DURATION_MINUTES));
            }

            return null;
        }
    }
}