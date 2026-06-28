using AutoServ.Core.Entities;
using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.DTOs.Consultations;
using AutoServ.DTOs.Masters;
using AutoServ.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class ConsultationService : IConsultationService
    {
        private readonly ApplicationContext _context;
        private readonly IScheduleService _scheduleService;
        private readonly IFileStorageService _fileStorageService;

        public ConsultationService(ApplicationContext context, IScheduleService scheduleService, IFileStorageService fileStorageService)
        {
            _context = context;
            _scheduleService = scheduleService;
            _fileStorageService = fileStorageService;
        }

        public async Task<Consultation> CreateAsync(CreateConsultationDto dto, IFormFile? attachmentFile)
        {
            bool existsToday = await _context.Consultations.AnyAsync(c =>
                c.UserId == dto.UserId &&
                c.ConsultationDate == dto.ConsultationDate &&
                c.Status != "deleted");

            if (existsToday)
                throw new InvalidOperationException("Ви вже створили консультацію на цю дату. Дозволена лише одна консультація на день.");

            string? attachmentUrl = null;

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                using var stream = attachmentFile.OpenReadStream();
                attachmentUrl = await _fileStorageService.UploadFileAsync(stream, "consultations", attachmentFile.FileName);
            }

            var consultation = new Consultation
            {
                UserId = dto.UserId,
                ProblemDescription = dto.ProblemDescription,
                ConsultationDate = dto.ConsultationDate,
                ConsultationTime = dto.ConsultationTime,
                Status = "new",
                AttachmentUrl = attachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Consultations.AddAsync(consultation);
            await _context.SaveChangesAsync();
            return consultation;
        }

        public async Task<List<MasterConsultationDto>> GetForMasterAsync(int userId)
        {
            var master = await _context.Masters.FirstOrDefaultAsync(m => m.UserId == userId);
            if (master == null) throw new Exception("Майстер не знайдений");

            var consultations = await _context.Consultations
                .Include(c => c.User)
                .Where(c => c.Status == "new" || (c.MasterId == master.Id && c.Status == "accepted"))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var userIds = consultations.Where(c => c.UserId != 0).Select(c => c.UserId).Distinct().ToList();
            var profiles = userIds.Any()
                ? await _context.Set<UserProfile>()
                    .Where(p => userIds.Contains(p.UserId))
                    .ToDictionaryAsync(p => p.UserId, p => p)
                : new Dictionary<int, UserProfile>();

            return consultations.Select(c =>
            {
                profiles.TryGetValue(c.UserId, out var profile);

                return new MasterConsultationDto
                {
                    Id = c.Id,
                    ClientName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Клієнт",
                    Phone = c.User != null ? c.User.Phone : "-",
                    Problem = c.ProblemDescription,
                    Date = c.ConsultationDate,
                    Time = c.ConsultationTime,
                    Status = c.Status,
                    AttachmentUrl = c.AttachmentUrl,
                    CarBrand = profile?.CarBrand,
                    CarModel = profile?.CarModel,
                    CarYear = profile?.CarYear,
                    CarEngine = profile?.CarEngine,
                    CarPlate = profile?.CarPlate,
                };
            }).ToList();
        }

        public async Task<Result<object>> AcceptAsync(int consultationId, int masterUserId)
        {
            var master = await _context.Masters.FirstOrDefaultAsync(m => m.UserId == masterUserId);
            if (master == null) return Result<object>.Failure("Майстер не знайдено");

            var consultation = await _context.Consultations
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == consultationId);
            if (consultation == null) return Result<object>.Failure("Консультацію не знайдено");
            if (consultation.MasterId != null) return Result<object>.Failure("Консультацію вже прийнято іншим майстром");

            DateTime dateCheck = consultation.ConsultationDate.ToDateTime(TimeOnly.MinValue);
            TimeSpan timeCheck = consultation.ConsultationTime.ToTimeSpan();

            bool isAvailable = await _scheduleService.IsTimeSlotAvailableAsync(master.Id, dateCheck, timeCheck, 30);
            if (!isAvailable) return Result<object>.Failure("Неможливо прийняти: цей час вже зайнятий у вашому графіку.");

            consultation.MasterId = master.Id;
            consultation.Status = "accepted";

            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.UserId == consultation.UserId &&
                b.BookingDate == dateCheck &&
                b.BookingType == "consultation");

            int bookingIdToReturn;
            if (existingBooking == null)
            {
                var newBooking = new Booking
                {
                    UserId = consultation.UserId,
                    MasterId = master.Id,
                    BookingDate = dateCheck,
                    BookingTime = timeCheck,
                    CustomerName = consultation.User != null ? $"{consultation.User.FirstName} {consultation.User.LastName}" : "Клієнт",
                    CustomerPhone = consultation.User?.Phone ?? "",
                    Comment = consultation.ProblemDescription,
                    Status = Core.Enums.BookingStatus.Confirmed,
                    BookingType = "consultation",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();
                bookingIdToReturn = newBooking.Id;
            }
            else
            {
                existingBooking.MasterId = master.Id;
                existingBooking.Status = Core.Enums.BookingStatus.Confirmed;
                bookingIdToReturn = existingBooking.Id;
            }

            await _context.SaveChangesAsync();

            return Result<object>.Success(new { consultation.Id, consultation.Status, consultation.MasterId, BookingId = bookingIdToReturn });
        }
    }
}