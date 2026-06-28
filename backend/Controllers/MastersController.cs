using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MastersController : ControllerBase
    {
        private readonly IMasterService _masterService;
        private readonly IScheduleService _scheduleService;

        public MastersController(IMasterService masterService, IScheduleService scheduleService)
        {
            _masterService = masterService;
            _scheduleService = scheduleService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetMasters() => Ok(await _masterService.GetAllActiveAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetMaster(int id)
        {
            var master = await _masterService.GetByIdAsync(id);
            return master == null ? NotFound(new { message = "Майстра не знайдено" }) : Ok(master);
        }

        [HttpGet("{id}/available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAvailableSlots(int id, [FromQuery] DateTime date)
        {
            try { return Ok(await _scheduleService.GetMasterScheduleAsync(id, date)); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}/specializations")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateSpecializations(int id, [FromBody] UpdateMasterSpecializationsDto dto)
        {
            var result = await _masterService.UpdateSpecializationsAsync(id, dto);
            return result.IsSuccess ? Ok(new { message = "Оновлено" }) : BadRequest(new { message = result.Error });
        }

        [HttpPost("{id}/specializations/{serviceItemId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddSpecialization(int id, int serviceItemId)
        {
            var result = await _masterService.AddSpecializationAsync(id, serviceItemId);
            return result.IsSuccess ? Ok(new { message = "Додано" }) : BadRequest(new { message = result.Error });
        }
    }
}
