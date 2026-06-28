using AutoServ.Core.Entities;
using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.Core.States;
using AutoServ.DTOs.Admin;
using AutoServ.DTOs.Bookings;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoServ.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationContext _context;
        private readonly IScheduleService _scheduleService;
        private readonly IMasterAllocationStrategy _allocationStrategy;

        private const int CONSULTATION_DURATION = 30;

        public BookingService(ApplicationContext context, IScheduleService scheduleService, IMasterAllocationStrategy allocationStrategy)
        {
            _context = context;
            _scheduleService = scheduleService;
            _allocationStrategy = allocationStrategy;
        }

        public async Task<List<Booking>> GetAllAsync() =>
            await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Include(b => b.Master)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<Booking?> GetByIdAsync(int id) =>
            await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Include(b => b.Recommendations).ThenInclude(r => r.ServiceItem)
                .Include(b => b.Master)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<Result<List<BookingHistoryDto>>> GetUserHistoryAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId && b.BookingType != "consultation")
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Include(b => b.Master)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var consultations = await _context.Consultations
                .Where(c => c.UserId == userId && c.Status != "deleted")
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var bookingDtos = bookings.Select(b => new BookingHistoryDto
            {
                Id = b.Id,
                BookingDate = b.BookingDate,
                BookingTime = b.BookingTime,
                CarPlate = b.CarPlate,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                BookingType = b.BookingType ?? "service",
                CreatedAt = b.CreatedAt,
                MasterName = b.Master != null ? $"{b.Master.FirstName} {b.Master.LastName}" : "Не призначено",
                Services = b.Services.Select(s => new HistoryServiceDto
                {
                    ServiceItemId = s.ServiceItemId,
                    Title = s.ServiceItem?.Title ?? "N/A",
                    PriceAtBooking = s.PriceAtBooking
                }).ToList(),
                ParentBookingId = b.ParentBookingId,
                ServiceDisplay = b.Services.Any() ? string.Join(", ", b.Services.Select(s => s.ServiceItem?.Title ?? "Послуга")) : "-",
                AttachmentUrl = null
            }).ToList();

            var consultationDtos = consultations.Select(c => new BookingHistoryDto
            {
                Id = c.Id,
                BookingDate = c.ConsultationDate.ToDateTime(TimeOnly.MinValue),
                BookingTime = c.ConsultationTime.ToTimeSpan(),
                CarPlate = null,
                TotalPrice = 0,
                Status = c.Status switch
                {
                    "new" => BookingStatus.Pending,
                    "accepted" => BookingStatus.Confirmed,
                    "completed" => BookingStatus.Completed,
                    "cancelled" or "rejected" => BookingStatus.Cancelled,
                    _ => BookingStatus.Pending
                },
                BookingType = "consultation",
                CreatedAt = c.CreatedAt,
                MasterName = c.Status switch
                {
                    "new" => "Очікує майстра",
                    "accepted" => "Прийнято",
                    "completed" => "Завершено",
                    _ => "Скасовано"
                },
                Services = new List<HistoryServiceDto> { new HistoryServiceDto { ServiceItemId = 0, Title = "Консультація", PriceAtBooking = 0 } },
                ParentBookingId = null,
                ServiceDisplay = "Консультація",
                AttachmentUrl = c.AttachmentUrl
            }).ToList();

            var allHistory = bookingDtos.Concat(consultationDtos).OrderByDescending(h => h.CreatedAt).ToList();
            return Result<List<BookingHistoryDto>>.Success(allHistory);
        }

        public async Task<Result<List<MasterScheduleTaskDto>>> GetMasterScheduleAsync(int userId, DateTime date)
        {
            var master = await _context.Masters.FirstOrDefaultAsync(m => m.UserId == userId);
            if (master == null)
                return Result<List<MasterScheduleTaskDto>>.Success(new List<MasterScheduleTaskDto>());

            var bookings = await _context.Bookings
                .Where(b => b.MasterId == master.Id && b.BookingDate.Date == date.Date)
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .OrderBy(b => b.BookingTime)
                .ToListAsync();

            var tasks = bookings.Select(b =>
            {
                int durationMins = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 0);
                if (durationMins == 0) durationMins = CONSULTATION_DURATION;

                var endTime = b.BookingTime.Add(TimeSpan.FromMinutes(durationMins));
                string serviceName = string.Join(", ", b.Services.Select(s => s.ServiceItem?.Title ?? "Послуга"));
                if (string.IsNullOrEmpty(serviceName))
                    serviceName = b.BookingType == "consultation" ? "Консультація" : "Запис без послуг";

                return new MasterScheduleTaskDto
                {
                    Id = b.Id,
                    Time = b.BookingTime.ToString(@"hh\:mm"),
                    EndTime = endTime.ToString(@"hh\:mm"),
                    Name = b.CustomerName,
                    Car = b.CarPlate ?? "Не вказано",
                    Service = serviceName,
                    Duration = $"{durationMins} хв",
                    Price = $"{b.TotalPrice} ₴",
                    Status = b.Status
                };
            }).ToList();

            return Result<List<MasterScheduleTaskDto>>.Success(tasks);
        }

        public async Task<Result<BookingCreatedResultDto>> CreateAsync(CreateBookingDto dto)
        {
            if (dto.UserId <= 0)
                return Result<BookingCreatedResultDto>.Failure("Користувач повинен бути авторизований");

            if (!TimeSpan.TryParse(dto.Time, out var parsedTime))
                return Result<BookingCreatedResultDto>.Failure("Невірний формат часу");

            
            string rawDate = dto.Date;
            DateTime parsedDate;

           
            if (rawDate.Contains("T") || rawDate.Contains("Z"))
            {
         
                if (DateTime.TryParse(rawDate, null, DateTimeStyles.RoundtripKind, out var utcDate))
                {
                  
                    parsedDate = utcDate.ToLocalTime().Date;
                }
                else
                {
                    parsedDate = DateTime.Today; // Запобіжник
                }
            }
            else
            {
               
                if (!DateTime.TryParseExact(rawDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedDate))
                {
                    return Result<BookingCreatedResultDto>.Failure("Невірний формат дати");
                }
            }

            DateTime currentDate = parsedDate;

            bool isConsultation = dto.Services == null || !dto.Services.Any();
            Dictionary<int, ServiceItem> dbServices = new Dictionary<int, ServiceItem>();

            if (!isConsultation)
                dbServices = await _context.ServiceItems.Where(s => dto.Services.Select(x => x.Id).Contains(s.Id)).ToDictionaryAsync(s => s.Id);

            if (isConsultation)
            {
                using var consultTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var booking = new Booking
                    {
                        UserId = dto.UserId,
                        MasterId = null,
                        BookingDate = currentDate,
                        BookingTime = parsedTime,
                        CustomerName = dto.Name,
                        CustomerPhone = dto.Phone,
                        CarPlate = dto.CarNumber,
                        Comment = dto.Comment,
                        TotalPrice = 0,
                        Status = BookingStatus.Pending,
                        BookingType = "consultation",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                    await consultTransaction.CommitAsync();

                    return Result<BookingCreatedResultDto>.Success(new BookingCreatedResultDto { OrderId = booking.Id, TotalPrice = 0 });
                }
                catch (Exception ex)
                {
                    await consultTransaction.RollbackAsync();
                    return Result<BookingCreatedResultDto>.Failure(ex.Message);
                }
            }

            var servicesList = dto.Services.Where(s => dbServices.ContainsKey(s.Id)).ToList();
            var plan = new List<(Master Master, List<BookingServiceItemDto> Services, TimeSpan StartTime)>();
            TimeSpan chainCursor = parsedTime;

            foreach (var svc in servicesList)
            {
                int duration = dbServices[svc.Id].DurationMinutes;
                var master = await _allocationStrategy.FindMasterAsync(svc.Id, currentDate, chainCursor, duration, plan);

                if (master == null)
                    return Result<BookingCreatedResultDto>.Failure($"на жаль, не вдалося знайти вільного майстра на \"{dbServices[svc.Id].Title}\" о {chainCursor:hh\\:mm}");

                plan.Add((master, new List<BookingServiceItemDto> { svc }, chainCursor));
                chainCursor = chainCursor.Add(TimeSpan.FromMinutes(duration));
            }

            return await SaveBookingPlanWithParent(plan, dto, currentDate);
        }

        private async Task<Result<BookingCreatedResultDto>> SaveBookingPlanWithParent(
            List<(Master Master, List<BookingServiceItemDto> Services, TimeSpan StartTime)> plan, CreateBookingDto dto, DateTime date)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var parentBooking = new Booking
                {
                    UserId = dto.UserId,
                    MasterId = null,
                    BookingDate = date,
                    BookingTime = plan.First().StartTime,
                    CustomerName = dto.Name,
                    CustomerPhone = dto.Phone,
                    CarPlate = dto.CarNumber,
                    Comment = dto.Comment,
                    TotalPrice = 0,
                    Status = BookingStatus.Confirmed,
                    BookingType = "service_order",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(parentBooking);
                await _context.SaveChangesAsync();

                decimal totalPrice = 0;
                var createdTasks = new List<CreatedTaskDto>();

                foreach (var item in plan)
                {
                    var childBooking = new Booking
                    {
                        ParentBookingId = parentBooking.Id,
                        UserId = dto.UserId,
                        MasterId = item.Master.Id,
                        BookingDate = date,
                        BookingTime = item.StartTime,
                        CustomerName = dto.Name,
                        CustomerPhone = dto.Phone,
                        CarPlate = dto.CarNumber,
                        Status = BookingStatus.Confirmed,
                        BookingType = "service_task",
                        TotalPrice = item.Services.Sum(s => s.Price),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Bookings.Add(childBooking);

                    foreach (var s in item.Services)
                    {
                        _context.BookingServices.Add(new Core.Entities.BookingService
                        {
                            Booking = childBooking,
                            ServiceItemId = s.Id,
                            PriceAtBooking = s.Price,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    totalPrice += childBooking.TotalPrice;
                    createdTasks.Add(new CreatedTaskDto
                    {
                        TaskId = childBooking.Id,
                        MasterId = item.Master.Id,
                        MasterName = $"{item.Master.FirstName} {item.Master.LastName}",
                        Time = item.StartTime.ToString(@"hh\:mm")
                    });
                }

                parentBooking.TotalPrice = totalPrice;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<BookingCreatedResultDto>.Success(new BookingCreatedResultDto { OrderId = parentBooking.Id, TotalPrice = totalPrice, Tasks = createdTasks });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<BookingCreatedResultDto>.Failure("Помилка: " + ex.Message);
            }
        }
        public async Task<List<AdminScheduleTaskDto>> GetAdminFullScheduleAsync(string? dateFilter = null)
        {
            DateTime targetDate = DateTime.Today;

           
            if (!string.IsNullOrEmpty(dateFilter))
            {
                dateFilter = dateFilter.Replace(".", "-");
                if (DateTime.TryParseExact(dateFilter, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    targetDate = parsedDate.Date;
                }
            }

            var nextDay = targetDate.AddDays(1);

            //ТІЛЬКИ "Дітей" (у них є MasterId) та консультації
            var tasks = await _context.Bookings
                .Include(b => b.Services).ThenInclude(s => s.ServiceItem)
                .Include(b => b.Master)
                .Where(b => b.MasterId != null &&
                            b.BookingDate >= targetDate &&
                            b.BookingDate < nextDay &&
                            b.Status != Core.Enums.BookingStatus.Cancelled)
                .ToListAsync();

            return tasks.Select(b => {
                int duration = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 30);
                return new AdminScheduleTaskDto
                {
                    Id = b.Id,
                    MasterName = b.Master != null ? $"{b.Master.FirstName} {b.Master.LastName}" : "Невідомий",
                    Time = b.BookingTime.ToString(@"hh\:mm"),
                    EndTime = b.BookingTime.Add(TimeSpan.FromMinutes(duration)).ToString(@"hh\:mm"),
                    ClientName = b.CustomerName,
                    CarInfo = b.CarPlate ?? "Не вказано",
                    Service = string.Join(", ", b.Services.Select(s => s.ServiceItem?.Title ?? "Послуга")),
                    // тут статус автоматично конвертується в snake_case завдяки налаштуванням Program.cs (in_progress)
                    Status = b.Status.ToString()
                };
            }).ToList();
        }

        public async Task<Result> UpdateTaskStatusAsync(int taskId, string newStatus)
        {
            if (!Enum.TryParse<BookingStatus>(newStatus.Replace("_", ""), ignoreCase: true, out var parsedStatus))
                return Result.Failure("unknown status");

            var task = await _context.Bookings.Include(b => b.ParentBooking).FirstOrDefaultAsync(b => b.Id == taskId);
            if (task == null) return Result.Failure("task not found");

            if (!BookingStateTransitions.CanTransition(task.Status, parsedStatus))
                return Result.Failure(BookingStateTransitions.GetErrorMessage(task.Status, parsedStatus));

            task.Status = parsedStatus;

            if (task.ParentBookingId.HasValue)
            {
                var allTasks = await _context.Bookings.Where(b => b.ParentBookingId == task.ParentBookingId).ToListAsync();
                var parent = task.ParentBooking;
                if (parent != null)
                {
                    bool allCompleted = allTasks.All(t => t.Status == BookingStatus.Completed);
                    bool anyInProgress = allTasks.Any(t => t.Status == BookingStatus.InProgress);

                    if (allCompleted) parent.Status = BookingStatus.Completed;
                    else if (anyInProgress) parent.Status = BookingStatus.InProgress;
                }
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        // ==========================================
        // МЕТОД ДЛЯ АДМІН-ТАБЛИЦІ (Фільтрування через рядок)
        // ==========================================
        public async Task<List<AdminBookingDto>> GetAdminTableAsync(string? dateFilter = null)
        {
            var query = _context.Bookings
            .Include(b => b.User).ThenInclude(u => u.Profile)
            .Include(b => b.Master) // Для батьківського запису
            .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem) // Послуги батька
            .Include(b => b.Children).ThenInclude(c => c.Services).ThenInclude(cs => cs.ServiceItem) // Послуги дітей
            .Include(b => b.Children).ThenInclude(c => c.Master) // МАЙСТРИ ДІТЕЙ (НОВЕ!)
            .Where(b => b.BookingType != "service_task");

            if (!string.IsNullOrEmpty(dateFilter))
            {
                query = query.Where(b => EF.Functions.Like(b.BookingDate.ToString(), $"{dateFilter}%"));
            }

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            return bookings.Select(b =>
            {

                string servicesStr = "Немає послуг";

                if (b.BookingType == "consultation")
                {
                    servicesStr = "Онлайн-консультація";
                }
                else if (b.Children != null && b.Children.Any())
                {
                    servicesStr = string.Join(", ", b.Children.SelectMany(c => c.Services).Select(s => s.ServiceItem?.Title ?? "Послуга"));
                }
                else if (b.Services != null && b.Services.Any())
                {
                    servicesStr = string.Join(", ", b.Services.Select(s => s.ServiceItem?.Title ?? "Послуга"));
                }

                string masterNamesStr = "Очікує призначення";

                if (b.BookingType == "consultation")
                {
                    masterNamesStr = b.Master != null ? $"{b.Master.FirstName} {b.Master.LastName}" : "Очікує майстра";
                }
                else if (b.Children != null && b.Children.Any())
                {
         
                    masterNamesStr = string.Join(", ", b.Children
                        .Where(c => c.Master != null)
                        .Select(c => $"{c.Master.FirstName} {c.Master.LastName}")
                        .Distinct());
                }
                else if (b.Master != null)
                {
           
                    masterNamesStr = $"{b.Master.FirstName} {b.Master.LastName}";
                }

                return new AdminBookingDto
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    BookingTime = b.BookingTime.ToString(@"hh\:mm"),
                    ClientName = b.CustomerName,
                    ClientPhone = b.CustomerPhone,

                    CarInfo = b.User?.Profile != null
                        ? $"{b.User.Profile.CarBrand} {b.User.Profile.CarModel} {b.User.Profile.CarPlate} {b.User.Profile.CarYear}"
                        : (string.IsNullOrEmpty(b.CarPlate) ? "Не вказано" : b.CarPlate),
                    Services = servicesStr,
                    MasterName = masterNamesStr, // Тепер підставляємо список майстрів
                    Status = b.Status,
                    BookingType = b.BookingType ?? "service"
                };
            }).ToList();
        }
    }
}