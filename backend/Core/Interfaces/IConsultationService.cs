using AutoServ.Core.Entities;
using AutoServ.Core.Models;
using AutoServ.DTOs.Consultations;
using AutoServ.DTOs.Masters;
using Microsoft.AspNetCore.Http;

namespace AutoServ.Core.Interfaces
{
    public interface IConsultationService
    {
        Task<Consultation> CreateAsync(CreateConsultationDto dto, IFormFile? attachmentFile);
        Task<List<MasterConsultationDto>> GetForMasterAsync(int userId);
        Task<Result<object>> AcceptAsync(int consultationId, int masterUserId);
    }
}