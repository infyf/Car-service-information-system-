using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.DTOs.Masters;
using AutoServ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Infrastructure.Services
{
    public class MasterService : IMasterService
    {
        private readonly ApplicationContext _context;

        public MasterService(ApplicationContext context) { _context = context; }

        public async Task<List<MasterDto>> GetAllActiveAsync()
        {
            return await _context.Masters.Where(m => m.IsActive)
                .Select(m => new MasterDto { Id = m.Id, FullName = $"{m.FirstName} {m.LastName}", Phone = m.Phone, ExperienceYears = m.ExperienceYears, IsActive = m.IsActive })
                .ToListAsync();
        }

        public async Task<object?> GetByIdAsync(int id)
        {
            var master = await _context.Masters.Include(m => m.ServiceItems).FirstOrDefaultAsync(m => m.Id == id);
            if (master == null) return null;

            return new
            {
                master.Id,
                FullName = $"{master.FirstName} {master.LastName}",
                master.Phone,
                master.ExperienceYears,
                master.IsActive,
                Specializations = master.ServiceItems.Select(s => new { s.Id, s.Title, s.PriceFrom, s.DurationMinutes })
            };
        }

        public async Task<Result> UpdateSpecializationsAsync(int id, UpdateMasterSpecializationsDto dto)
        {
            var master = await _context.Masters.Include(m => m.ServiceItems).FirstOrDefaultAsync(m => m.Id == id);
            if (master == null) return Result.Failure("master not found");

            master.ServiceItems = await _context.ServiceItems.Where(s => dto.ServiceItemIds.Contains(s.Id)).ToListAsync();
            try { await _context.SaveChangesAsync(); return Result.Success(); }
            catch (DbUpdateException ex) { return Result.Failure("error while saving: " + ex.InnerException?.Message); }
        }

        public async Task<Result> AddSpecializationAsync(int id, int serviceItemId)
        {
            var master = await _context.Masters.Include(m => m.ServiceItems).FirstOrDefaultAsync(m => m.Id == id);
            if (master == null) return Result.Failure("master not found");
            if (master.ServiceItems.Any(s => s.Id == serviceItemId)) return Result.Failure("ця спеціалізація вже є у майстра");

            var service = await _context.ServiceItems.FindAsync(serviceItemId);
            if (service == null) return Result.Failure("service not found");

            master.ServiceItems.Add(service);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}