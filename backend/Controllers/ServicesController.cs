using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Admin; // ДОДАНО
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService) { _serviceService = serviceService; }

        [HttpGet]
        public async Task<IActionResult> GetServices() => Ok(await _serviceService.GetAllAsync());

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetServiceDetail(int id)
        {
            var dto = await _serviceService.GetDetailAsync(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        [HttpGet("with-items")]
        public async Task<IActionResult> GetServicesWithItems() => Ok(await _serviceService.GetWithItemsAsync());

        // ЕНДПОІНТИ ДЛЯ АДМІНА (КЕРУВАННЯ ПОСЛУГАМИ)
        
        
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateService([FromBody] ManageServiceDto dto)
        {
            var result = await _serviceService.CreateAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error });
            return Ok(new { message = "Послугу успішно створено" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ManageServiceDto dto)
        {
            var result = await _serviceService.UpdateAsync(id, dto);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error });
            return Ok(new { message = "Послугу оновлено" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var result = await _serviceService.DeleteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error });
            return Ok(new { message = "Послугу видалено" });
        }
    }
}