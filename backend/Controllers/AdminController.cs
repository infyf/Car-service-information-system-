using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IAdminServiceService _adminServiceService;

        
        public AdminController(
            IBookingService bookingService,
            IAdminServiceService adminServiceService)
        {
            _bookingService = bookingService;
            _adminServiceService = adminServiceService;
        }


        [HttpGet("masters-stats")]
        public async Task<IActionResult> GetMastersStats([FromQuery] string? date = null)
        {
            var stats = await _adminServiceService.GetDashboardStatsAsync(date);
            return Ok(stats);
        }


        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookingsTable([FromQuery] string? dateFilter = null)
        {
            var data = await _bookingService.GetAdminTableAsync(dateFilter);
            return Ok(data);
        }

      

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _adminServiceService.GetClientsAsync();
            return Ok(clients);
        }

     

        [HttpGet("masters-today-schedule")]
        public async Task<IActionResult> GetMastersTodaySchedule([FromQuery] string? date = null)
        {
            var schedule = await _adminServiceService.GetMastersScheduleAsync(date);
            return Ok(schedule);
        }

        [HttpGet("full-schedule")]
        public async Task<IActionResult> GetFullSchedule([FromQuery] string? date = null)
        {
            var data = await _bookingService.GetAdminFullScheduleAsync(date);
            return Ok(data);
        }

      

        [HttpGet("masters-detailed")]
        public async Task<IActionResult> GetMastersDetailed()
        {
            var masters = await _adminServiceService.GetMastersDetailedAsync();
            return Ok(masters);
        }

        [HttpPatch("masters/{id}/toggle-status")]
        public async Task<IActionResult> ToggleMasterStatus(int id)
        {
            var result = await _adminServiceService.ToggleMasterStatusAsync(id);
            return result.IsSuccess
                ? Ok(new { message = "Статус успішно оновлено" })
                : BadRequest(new { message = result.Error });
        }

        [HttpDelete("masters/{masterId}/specializations/{serviceItemId}")]
        public async Task<IActionResult> RemoveSpecialization(int masterId, int serviceItemId)
        {
            var result = await _adminServiceService.RemoveSpecializationAsync(masterId, serviceItemId);
            return result.IsSuccess
                ? Ok(new { message = "Спеціалізацію видалено" })
                : BadRequest(new { message = result.Error });
        }


 
    

        [HttpGet("services")]
        public async Task<IActionResult> GetAllServices()
        {
            var data = await _adminServiceService.GetAllForAdminAsync();
            return Ok(data);
        }

        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] ManageServiceDto dto)
        {
            var result = await _adminServiceService.CreateServiceAsync(dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ManageServiceDto dto)
        {
            var result = await _adminServiceService.UpdateServiceAsync(id, dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var result = await _adminServiceService.DeleteServiceAsync(id);
            return result.IsSuccess ? Ok(new { message = "Видалено" }) : BadRequest(new { message = result.Error });
        }

        [HttpPost("services/{serviceId}/items")]
        public async Task<IActionResult> AddItem(int serviceId, [FromBody] ManageServiceItemDto dto)
        {
            var result = await _adminServiceService.AddItemAsync(serviceId, dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ManageServiceItemDto dto)
        {
            var result = await _adminServiceService.UpdateItemAsync(id, dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var result = await _adminServiceService.DeleteItemAsync(id);
            return result.IsSuccess ? Ok(new { message = "Видалено" }) : BadRequest(new { message = result.Error });
        }

       
        //  (inline editing)
        

        [HttpPut("services/{id}/description")]
        public async Task<IActionResult> UpdateServiceDescription(int id, [FromBody] UpdateServiceDescriptionDto dto)
        {
            var result = await _adminServiceService.UpdateServiceDescriptionAsync(id, dto.Description);
            return result.IsSuccess ? Ok(new { message = "Опис успішно оновлено" }) : BadRequest(new { message = result.Error });
        }

        [HttpPut("services/{id}/title")]
        public async Task<IActionResult> UpdateServiceTitle(int id, [FromBody] UpdateServiceTitleDto dto)
        {
            var result = await _adminServiceService.UpdateServiceTitleAsync(id, dto.Title);
            return result.IsSuccess ? Ok(new { message = "Назву успішно оновлено" }) : BadRequest(new { message = result.Error });
        }
    }
}