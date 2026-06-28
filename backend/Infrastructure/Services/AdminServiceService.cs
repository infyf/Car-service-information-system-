using AutoServ.Core.Entities;
using AutoServ.Core.Enums;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.DTOs.Admin;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoServ.Infrastructure.Services
{
    public class AdminServiceService : IAdminServiceService
    {
        private readonly ApplicationContext _context;

        public AdminServiceService(ApplicationContext context)
        {
            _context = context;
        }



        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(string? date = null)
        {
            DateTime targetDate = DateTime.Today;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                targetDate = parsedDate.Date;

            var nextDay = targetDate.AddDays(1);
            var totalMasters = await _context.Masters.CountAsync(m => m.IsActive);

            var scheduledTasksCount = await _context.Bookings
                .Where(b => b.BookingDate >= targetDate && b.BookingDate < nextDay && b.MasterId != null &&
                            b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed).CountAsync();

            var pendingTasksCount = await _context.Bookings
                .Where(b => b.BookingDate >= targetDate && b.BookingDate < nextDay && b.MasterId != null &&
                            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)).CountAsync();

            var completedTasksCount = await _context.Bookings
                .Where(b => b.BookingDate >= targetDate && b.BookingDate < nextDay && b.MasterId != null &&
                            b.Status == BookingStatus.Completed).CountAsync();

            var busyMasters = await _context.Bookings
                .Where(b => b.BookingDate >= targetDate && b.BookingDate < nextDay && b.MasterId != null &&
                            b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed)
                .Select(b => b.MasterId).Distinct().CountAsync();

            return new AdminDashboardStatsDto
            {
                TotalMasters = totalMasters,
                ActiveMasters = totalMasters,
                BusyMastersToday = busyMasters,
                FreeMastersToday = totalMasters - busyMasters,
                ScheduledToday = scheduledTasksCount,
                PendingToday = pendingTasksCount,
                CompletedToday = completedTasksCount
            };
        }

        public async Task<List<AdminClientDto>> GetClientsAsync()
        {
            return await _context.Users
                .Where(u => u.Role.ToLower() == "client")
                .Include(u => u.Profile)
                .Select(u => new AdminClientDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Phone = u.Phone ?? "-",
                    CarInfo = u.Profile != null ? $"{u.Profile.CarBrand} {u.Profile.CarModel} {u.Profile.CarPlate} ({u.Profile.CarYear})" : "Не вказано",
                    RegisteredAt = u.CreatedAt,
                    TotalBookings = _context.Bookings.Count(b => b.UserId == u.Id && b.BookingType == "service_order")
                })
                .OrderByDescending(c => c.RegisteredAt)
                .ToListAsync();
        }

        public async Task<List<AdminMasterScheduleDto>> GetMastersScheduleAsync(string? date = null)
        {
            DateTime targetDate = DateTime.Today;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                targetDate = parsedDate.Date;

            var nextDay = targetDate.AddDays(1);

            var masters = await _context.Masters
                .Where(m => m.IsActive)
                .Select(m => new { m.Id, FullName = m.FirstName + " " + m.LastName })
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Services).ThenInclude(bs => bs.ServiceItem)
                .Where(b => b.BookingDate >= targetDate && b.BookingDate < nextDay && b.MasterId != null &&
                            b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed)
                .ToListAsync();

            return masters.Select(m => {
                var masterBookings = bookings.Where(b => b.MasterId == m.Id).OrderBy(b => b.BookingTime).ToList();
                var slots = masterBookings.Select(b => {
                    int duration = b.Services.Sum(s => s.ServiceItem?.DurationMinutes ?? 30);
                    return new AdminMasterSlotDto
                    {
                        Start = b.BookingTime.ToString(@"hh\:mm"),
                        End = b.BookingTime.Add(TimeSpan.FromMinutes(duration)).ToString(@"hh\:mm")
                    };
                }).ToList();

                return new AdminMasterScheduleDto
                {
                    Id = m.Id,
                    FullName = m.FullName,
                    IsFree = slots.Count == 0,
                    Slots = slots
                };
            }).OrderByDescending(m => m.Slots.Count).ToList();
        }

        public async Task<List<AdminMasterDetailedDto>> GetMastersDetailedAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Masters
                .Include(m => m.ServiceItems)
                .Include(m => m.User)
                .Include(m => m.Bookings)
                .OrderByDescending(m => m.IsActive)
                .Select(m => new AdminMasterDetailedDto
                {
                    Id = m.Id,
                    FullName = m.FirstName + " " + m.LastName,
                    Phone = m.Phone,
                    Email = m.User != null ? m.User.Email : null,
                    ExperienceYears = m.ExperienceYears,
                    IsActive = m.IsActive,
                    IsBusyToday = m.Bookings != null && m.Bookings.Any(b => b.BookingDate >= today && b.BookingDate < tomorrow && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Completed),
                    TotalCompletedBookings = m.Bookings != null ? m.Bookings.Count(b => b.Status == BookingStatus.Completed) : 0,
                    Specializations = m.ServiceItems.Select(s => new MasterSpecDto { Id = s.Id, Title = s.Title }).ToList()
                }).ToListAsync();
        }

        public async Task<Result> ToggleMasterStatusAsync(int id)
        {
            var master = await _context.Masters.FindAsync(id);
            if (master == null) return Result.Failure("Майстра не знайдено");

            master.IsActive = !master.IsActive;
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> RemoveSpecializationAsync(int masterId, int serviceItemId)
        {
            var master = await _context.Masters.Include(m => m.ServiceItems).FirstOrDefaultAsync(m => m.Id == masterId);
            if (master == null) return Result.Failure("Майстра не знайдено");

            var serviceToRemove = master.ServiceItems.FirstOrDefault(s => s.Id == serviceItemId);
            if (serviceToRemove == null) return Result.Failure("У майстра немає такої спецалізації");

            master.ServiceItems.Remove(serviceToRemove);
            await _context.SaveChangesAsync();
            return Result.Success();
        }



        public async Task<Result<object>> CreateServiceAsync(ManageServiceDto dto)
        {
            var service = new Service
            {
                Title = dto.Title,
                Slug = dto.Slug,
                ServiceDetail = new ServiceDetail
                {
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    PriceFrom = dto.PriceFrom,
                    Duration = dto.Duration,
                    Warranty = dto.Warranty
                }
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return Result<object>.Success(new { service.Id, service.Title });
        }

        public async Task<Result<object>> UpdateServiceAsync(int serviceId, ManageServiceDto dto)
        {
            var service = await _context.Services.Include(s => s.ServiceDetail).FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null) return Result<object>.Failure("Не знайдено");

            service.Title = dto.Title;
            service.Slug = dto.Slug;

            if (service.ServiceDetail == null)
            {
                service.ServiceDetail = new ServiceDetail { ServiceId = serviceId };
                _context.ServiceDetails.Add(service.ServiceDetail);
            }

            service.ServiceDetail.Description = dto.Description;
            service.ServiceDetail.ImageUrl = dto.ImageUrl;
            service.ServiceDetail.PriceFrom = dto.PriceFrom;
            service.ServiceDetail.Duration = dto.Duration;
            service.ServiceDetail.Warranty = dto.Warranty;

            await _context.SaveChangesAsync();
            return Result<object>.Success(new { message = "Оновлено" });
        }

        public async Task<Result> DeleteServiceAsync(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return Result.Failure("Не знайдено");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<object>> AddItemAsync(int serviceId, ManageServiceItemDto dto)
        {
            var exists = await _context.Services.AnyAsync(s => s.Id == serviceId);
            if (!exists) return Result<object>.Failure("Категорію не знайдено");

            var item = new ServiceItem
            {
                ServiceId = serviceId,
                Title = dto.Title,
                PriceFrom = dto.PriceFrom,
                DurationMinutes = dto.DurationMinutes
            };

            _context.ServiceItems.Add(item);
            await _context.SaveChangesAsync();

            return Result<object>.Success(new { item.Id, item.Title, item.DurationMinutes });
        }

        public async Task<Result<object>> UpdateItemAsync(int itemId, ManageServiceItemDto dto)
        {
            var item = await _context.ServiceItems.FindAsync(itemId);
            if (item == null) return Result<object>.Failure("Послугу не знайдено");

            item.Title = dto.Title;
            item.PriceFrom = dto.PriceFrom;
            item.DurationMinutes = dto.DurationMinutes;

            await _context.SaveChangesAsync();
            return Result<object>.Success(new { message = "Час та ціна оновлені" });
        }

        public async Task<Result> DeleteItemAsync(int itemId)
        {
            var item = await _context.ServiceItems.FindAsync(itemId);
            if (item == null) return Result.Failure("Послугу не знайдено");

            _context.ServiceItems.Remove(item);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<List<AdminServiceListDto>> GetAllForAdminAsync()
        {
            return await _context.Services
                .Include(s => s.ServiceDetail)
                .Include(s => s.Items)
                .OrderBy(s => s.Id)
                .Select(s => new AdminServiceListDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Slug = s.Slug,
                    Description = s.ServiceDetail != null ? s.ServiceDetail.Description : "",
                    ImageUrl = s.ServiceDetail != null ? s.ServiceDetail.ImageUrl : "",
                    Items = s.Items.Select(i => new AdminServiceItemDto
                    {
                        Id = i.Id,
                        Title = i.Title,
                        PriceFrom = i.PriceFrom,
                        DurationMinutes = i.DurationMinutes
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<Result> UpdateServiceDescriptionAsync(int serviceId, string description)
        {
            var service = await _context.Services
                .Include(s => s.ServiceDetail)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null) return Result.Failure("Категорію не знайдено");

            if (service.ServiceDetail == null)
            {
                service.ServiceDetail = new ServiceDetail
                {
                    ServiceId = serviceId,
                    Description = description,
                    ImageUrl = "",
                    PriceFrom = 0,
                    Duration = "Не вказано",
                    Warranty = "Не вказано"
                };
                _context.ServiceDetails.Add(service.ServiceDetail);
            }
            else
            {
                service.ServiceDetail.Description = description;
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateServiceTitleAsync(int serviceId, string title)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return Result.Failure("Категорію не знайдено");

            service.Title = title;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}