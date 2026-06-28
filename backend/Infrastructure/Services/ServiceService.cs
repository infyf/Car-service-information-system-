using AutoServ.Core.Entities;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;      
using AutoServ.DTOs.Admin;      
using AutoServ.DTOs.Services;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ApplicationContext _context;
        public ServiceService(ApplicationContext context) { _context = context; }

        public async Task<List<object>> GetAllAsync()
        {
            var services = await _context.Services.Include(s => s.ServiceDetail).AsNoTracking().ToListAsync();
            return services.Select(s => (object)new
            {
                s.Id,
                s.Title,
                s.Slug,
                Detail = s.ServiceDetail == null ? null : new { s.ServiceDetail.Description, s.ServiceDetail.ImageUrl, s.ServiceDetail.PriceFrom, s.ServiceDetail.Duration, s.ServiceDetail.Warranty }
            }).ToList();
        }

        public async Task<ServiceDetailDto?> GetDetailAsync(int id)
        {
            var service = await _context.Services.Include(s => s.ServiceDetail).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (service == null || service.ServiceDetail == null) return null;

            return new ServiceDetailDto { ServiceId = service.Id, Title = service.Title, Description = service.ServiceDetail.Description, ImageUrl = service.ServiceDetail.ImageUrl, PriceFrom = service.ServiceDetail.PriceFrom, Duration = service.ServiceDetail.Duration, Warranty = service.ServiceDetail.Warranty };
        }

        public async Task<List<object>> GetWithItemsAsync()
        {
            var services = await _context.Services.Include(s => s.Items).AsNoTracking().ToListAsync();
            return services.Select(s => (object)new
            {
                s.Id,
                s.Title,
                s.Slug,
                Items = s.Items.Select(i => new { i.Id, i.Title, i.PriceFrom })
            }).ToList();
        }

 
        public async Task<Result> CreateAsync(ManageServiceDto dto)
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
                    Duration = dto.Duration ?? "60 хв",
                    Warranty = dto.Warranty
                }
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateAsync(int id, ManageServiceDto dto)
        {
            var service = await _context.Services.Include(s => s.ServiceDetail).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return Result.Failure("Послугу не знайдено");

            service.Title = dto.Title;
            service.Slug = dto.Slug;

            if (service.ServiceDetail != null)
            {
                service.ServiceDetail.Description = dto.Description;
                service.ServiceDetail.ImageUrl = dto.ImageUrl;
                service.ServiceDetail.PriceFrom = dto.PriceFrom;
                service.ServiceDetail.Duration = dto.Duration ?? "60 хв";
                service.ServiceDetail.Warranty = dto.Warranty;
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var service = await _context.Services.Include(s => s.ServiceDetail).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return Result.Failure("Послугу не знайдено");

            if (service.ServiceDetail != null)
                _context.ServiceDetails.Remove(service.ServiceDetail);

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}