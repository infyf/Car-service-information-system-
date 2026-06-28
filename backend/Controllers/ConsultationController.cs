using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Consultations;
using AutoServ.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AutoServ.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;

        public ConsultationsController(IConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        [HttpPost]
        [Authorize]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<IActionResult> Create([FromForm] CreateConsultationDto dto, IFormFile? attachmentFile)
        {
            dto.UserId = User.GetUserId();

            
            try
            {
                var consultation = await _consultationService.CreateAsync(dto, attachmentFile);
                return Ok(consultation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-list")]
        [Authorize(Roles = "master")]
        public async Task<IActionResult> GetMyConsultations()
        {
            int userId = User.GetUserId();
            return Ok(await _consultationService.GetForMasterAsync(userId));
        }

        [HttpPut("{id:int}/accept")]
        [Authorize(Roles = "master")]
        public async Task<IActionResult> Accept(int id)
        {
            int userId = User.GetUserId();
            var result = await _consultationService.AcceptAsync(id, userId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(result.Value);
        }
    }
}