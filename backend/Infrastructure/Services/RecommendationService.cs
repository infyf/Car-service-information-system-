using AutoServ.Core.Entities;
using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.DTOs.Recommendations;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationContext _context;
        private readonly IScheduleService _scheduleService;
        private readonly IMasterAllocationStrategy _allocationStrategy;

        private static readonly TimeSpan WORK_DAY_START = new TimeSpan(9, 0, 0);
        private static readonly TimeSpan WORK_DAY_END = new TimeSpan(18, 0, 0);

        public RecommendationService(ApplicationContext context, IScheduleService scheduleService, IMasterAllocationStrategy allocationStrategy)
        {
            _context = context;
            _scheduleService = scheduleService;
            _allocationStrategy = allocationStrategy;
        }

        public async Task<Recommendation> CreateAsync(CreateRecommendationDto dto)
        {
            if (dto == null || dto.BookingId <= 0 || dto.ServiceItemId <= 0) throw new ArgumentException("Некоректні дані");
            var rec = new Recommendation { BookingId = dto.BookingId, ServiceItemId = dto.ServiceItemId, Comment = dto.Comment ?? "", IsAccepted = false };
            _context.Recommendations.Add(rec);
            await _context.SaveChangesAsync();
            return rec;
        }

        public async Task<List<Recommendation>> GetByBookingAsync(int bookingId) =>
            await _context.Recommendations.Include(r => r.ServiceItem).Where(r => r.BookingId == bookingId && !r.IsAccepted).ToListAsync();

        public async Task<List<Recommendation>> GetByUserAsync(int userId) =>
            await _context.Recommendations.Include(r => r.ServiceItem).Include(r => r.Booking).Where(r => r.Booking.UserId == userId && !r.IsAccepted).ToListAsync();

        public async Task<Result<object>> AcceptAsync(AcceptRecommendationDto dto, int userId)
        {
            var recommendation = await _context.Recommendations.Include(r => r.ServiceItem).Include(r => r.Booking).FirstOrDefaultAsync(r => r.Id == dto.RecommendationId);
            if (recommendation == null) return Result<object>.Failure("не знайдено");
            if (recommendation.IsAccepted) return Result<object>.Failure("вже прийнята");
            if (recommendation.Booking.UserId != userId) return Result<object>.Failure("доступ заборонено");

            int duration = recommendation.ServiceItem.DurationMinutes > 0 ? recommendation.ServiceItem.DurationMinutes : 60;
            TimeSpan parsedTime;
            DateTime bookingDate;

            if (!dto.BookingDate.HasValue || string.IsNullOrEmpty(dto.BookingTime))
            {
                var autoResult = await FindBestAutoSlot(recommendation.ServiceItemId, duration);
                if (autoResult == null) return Result<object>.Failure("Немає вільних слотів");
                bookingDate = autoResult.Value.Date;
                parsedTime = autoResult.Value.Time;
            }
            else
            {
                bookingDate = dto.BookingDate.Value;
                if (!TimeSpan.TryParse(dto.BookingTime, out parsedTime)) return Result<object>.Failure("Невірний формат часу");
            }

            var master = await _allocationStrategy.FindMasterAsync(recommendation.ServiceItemId, bookingDate, parsedTime, duration);
            if (master == null) return Result<object>.Failure("Не вдалося знайти майстра");


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                CreateBookingEntity(recommendation, master, bookingDate, parsedTime);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Result<object>.Success(new { message = $"Записано на {bookingDate:dd.MM.yyyy} о {parsedTime:hh\\:mm}" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<object>.Failure("Помилка збереження: " + ex.Message);
            }
        }

        // PREVIEW (ONLY CALCULATION, WITHOUT DATABASE RECORDING))

        public async Task<Result<BatchPreviewDto>> PreviewBatchAsync(AcceptBatchRecommendationsDto dto, int userId)
        {
            var recommendations = await GetValidRecommendations(dto.RecommendationIds, userId);
            if (!recommendations.Any()) return Result<BatchPreviewDto>.Failure("Не знайдено або вже прийняті");

            var (bookingDate, chainCursor) = await GetInitialSlot(dto, recommendations.First());
            var previewItems = new List<PreviewItemDto>();

            foreach (var rec in recommendations)
            {
                int duration = rec.ServiceItem.DurationMinutes > 0 ? rec.ServiceItem.DurationMinutes : 60;
                var (master, newCursor) = await FindSlotForService(rec.ServiceItemId, bookingDate, chainCursor, duration);

                if (master == null) continue; // Якщо не знайшли - просто пропускаємо послугу

                previewItems.Add(new PreviewItemDto
                {
                    RecommendationId = rec.Id,
                    ServiceTitle = rec.ServiceItem.Title,
                    StartTime = newCursor.ToString(@"hh\:mm"),
                    EndTime = newCursor.Add(TimeSpan.FromMinutes(duration)).ToString(@"hh\:mm"),
                    MasterName = $"{master.FirstName} {master.LastName}",
                    Price = rec.ServiceItem.PriceFrom
                });
                chainCursor = newCursor.Add(TimeSpan.FromMinutes(duration));
            }

            if (!previewItems.Any()) return Result<BatchPreviewDto>.Failure("Неможливо підібрати час");

            return Result<BatchPreviewDto>.Success(new BatchPreviewDto
            {
                ProposedDate = bookingDate.ToString("dd.MM.yyyy"),
                StartTime = previewItems.First().StartTime,
                EndTime = previewItems.Last().EndTime,
                Items = previewItems
            });
        }

        public async Task<Result<BatchAcceptResultDto>> AcceptBatchAsync(AcceptBatchRecommendationsDto dto, int userId)
        {
            var recommendations = await GetValidRecommendations(dto.RecommendationIds, userId);
            if (!recommendations.Any()) return Result<BatchAcceptResultDto>.Failure("Не знайдено або вже прийняті");

            var result = new BatchAcceptResultDto { AcceptedItems = new List<AcceptedItemDto>(), FailedItems = new List<FailedItemDto>() };
            var (bookingDate, chainCursor) = await GetInitialSlot(dto, recommendations.First());

           
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var rec in recommendations)
                {
                    int duration = rec.ServiceItem.DurationMinutes > 0 ? rec.ServiceItem.DurationMinutes : 60;
                    var (master, newCursor) = await FindSlotForService(rec.ServiceItemId, bookingDate, chainCursor, duration);

                    if (master == null)
                    {
                        result.FailedItems.Add(new FailedItemDto { RecommendationId = rec.Id, ServiceTitle = rec.ServiceItem.Title, Reason = "Немає вільного майстра" });
                        chainCursor = newCursor.Add(TimeSpan.FromMinutes(duration)); // Все одно просуваємо час
                        continue;
                    }

           
                    CreateBookingEntity(rec, master, bookingDate, newCursor);

                    result.AcceptedItems.Add(new AcceptedItemDto
                    {
                        RecommendationId = rec.Id,
                        ServiceTitle = rec.ServiceItem.Title,
                        Date = bookingDate.ToString("dd.MM.yyyy"),
                        Time = newCursor.ToString(@"hh\:mm"),
                        MasterName = $"{master.FirstName} {master.LastName}"
                    });

                    chainCursor = newCursor.Add(TimeSpan.FromMinutes(duration));
                }

               
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Message = result.AcceptedItems.Any()
                    ? $"Успішно записано на {result.AcceptedItems.Count} послуг"
                    : "Не вдалося створити жодного запису";

                return Result<BatchAcceptResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<BatchAcceptResultDto>.Failure("Помилка бази даних при записі. Спробуйте ще раз.");
            }
        }

        private async Task<List<Recommendation>> GetValidRecommendations(List<int> ids, int userId)
        {
            if (ids == null || !ids.Any()) return new List<Recommendation>();
            return await _context.Recommendations
                .Include(r => r.ServiceItem).Include(r => r.Booking)
                .Where(r => ids.Contains(r.Id) && !r.IsAccepted && r.Booking.UserId == userId).ToListAsync();
        }

        private async Task<(DateTime Date, TimeSpan Time)> GetInitialSlot(AcceptBatchRecommendationsDto dto, Recommendation firstRec)
        {
            if (dto.PreferredDate.HasValue && !string.IsNullOrEmpty(dto.PreferredTime) && TimeSpan.TryParse(dto.PreferredTime, out var time))
                return (dto.PreferredDate.Value, time);

            var slot = await FindBestAutoSlot(firstRec.ServiceItemId, firstRec.ServiceItem.DurationMinutes);
            return slot ?? (DateTime.Today, WORK_DAY_START);
        }

        private async Task<(Master? Master, TimeSpan NewCursor)> FindSlotForService(int serviceId, DateTime date, TimeSpan cursor, int duration)
        {
            var master = await _allocationStrategy.FindMasterAsync(serviceId, date, cursor, duration, null);
            if (master != null) return (master, cursor);

            var slot = await FindAvailableSlotWithMaster(serviceId, date, cursor, duration);
            if (slot.HasValue)
            {
                var newCursor = slot.Value;
                master = await _allocationStrategy.FindMasterAsync(serviceId, date, newCursor, duration, null);
                return (master, newCursor);
            }
            return (null, cursor);
        }

        private async Task<(DateTime Date, TimeSpan Time)?> FindBestAutoSlot(int serviceItemId, int duration)
        {
            var now = DateTime.Now;
            var searchFrom = now.TimeOfDay.Add(TimeSpan.FromMinutes(30));
            if (searchFrom < WORK_DAY_START) searchFrom = WORK_DAY_START;

            int totalMinutes = (int)searchFrom.TotalMinutes;
            int remainder = totalMinutes % 30;
            if (remainder > 0) totalMinutes += (30 - remainder);
            searchFrom = new TimeSpan(totalMinutes / 60, totalMinutes % 60, 0);

            for (int i = 0; i < 3; i++)
            {
                var date = now.Date.AddDays(i);
                var startTime = (i == 0) ? searchFrom : WORK_DAY_START;
                var slot = await FindAvailableSlotWithMaster(serviceItemId, date, startTime, duration);
                if (slot.HasValue) return (date, slot.Value);
            }
            return null;
        }

        private async Task<TimeSpan?> FindAvailableSlotWithMaster(int serviceItemId, DateTime date, TimeSpan searchFrom, int duration)
        {
            var candidates = await _context.Masters.Include(m => m.ServiceItems).Where(m => m.IsActive && m.ServiceItems.Any(s => s.Id == serviceItemId)).ToListAsync();
            TimeSpan? earliestSlot = null;

            foreach (var m in candidates)
            {
                var slot = await _scheduleService.FindFirstAvailableSlotAsync(m.Id, date, duration, searchFrom);
                if (slot.HasValue && (!earliestSlot.HasValue || slot < earliestSlot)) earliestSlot = slot;
            }
            return earliestSlot;
        }

        

        private void CreateBookingEntity(Recommendation recommendation, Master master, DateTime bookingDate, TimeSpan parsedTime)
        {
            parsedTime = new TimeSpan(parsedTime.Hours, parsedTime.Minutes, 0);

            var newBooking = new Booking
            {
                UserId = recommendation.Booking.UserId,
                MasterId = master.Id,
                CustomerName = recommendation.Booking.CustomerName,
                CustomerPhone = recommendation.Booking.CustomerPhone,
                CarPlate = recommendation.Booking.CarPlate,
                Comment = $"Рекомендація #{recommendation.Id}",
                BookingDate = bookingDate,
                BookingTime = parsedTime,
                BookingType = "service_task",
                ParentBookingId = recommendation.BookingId,
                Status = BookingStatus.Confirmed,
                TotalPrice = recommendation.ServiceItem.PriceFrom,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(newBooking);

            _context.BookingServices.Add(new Core.Entities.BookingService
            {
                Booking = newBooking,
                ServiceItemId = recommendation.ServiceItemId,
                PriceAtBooking = recommendation.ServiceItem.PriceFrom,
                CreatedAt = DateTime.UtcNow
            });

            recommendation.IsAccepted = true;
            recommendation.CreatedBookingId = newBooking.Id;
        }
    }
}