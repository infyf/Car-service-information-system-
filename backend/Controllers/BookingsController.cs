using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll() => Ok(await _bookingService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _bookingService.GetByIdAsync(id);
            if (booking == null) return NotFound(new { message = $"бронювання №{id} NotFound" });

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("admin") && booking.UserId.ToString() != currentUserId) return Forbid();

            return Ok(new
            {
                booking.Id,
                booking.UserId,
                booking.CustomerName,
                booking.CustomerPhone,
                booking.CarPlate,
                booking.BookingDate,
                booking.BookingTime,
                booking.TotalPrice,
                booking.Status,
                booking.BookingType,
                booking.Comment,
                booking.ParentBookingId,
                MasterName = booking.Master != null ? $"{booking.Master.FirstName} {booking.Master.LastName}" : "Не призначено",
                Services = booking.Services.Select(s => new { s.ServiceItemId, Name = s.ServiceItem?.Title ?? "Послугу видалено", s.PriceAtBooking }),
                Recommendations = booking.Recommendations.Select(r => new { r.Id, r.ServiceItemId, ServiceTitle = r.ServiceItem?.Title ?? "Послугу видалено", r.Comment, r.IsAccepted, r.CreatedBookingId })
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId.ToString() && !User.IsInRole("admin")) return Forbid();

            var result = await _bookingService.GetUserHistoryAsync(userId);
            return Ok(result.Value);
        }

        [HttpGet("my-schedule")]
        [Authorize(Roles = "master, admin")]
        public async Task<IActionResult> GetMySchedule([FromQuery] int userId, [FromQuery] DateTime? date)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId.ToString() && !User.IsInRole("admin")) return Forbid();

            var result = await _bookingService.GetMasterScheduleAsync(userId, date ?? DateTime.UtcNow);
            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized(new { message = "помилка авторизації" });

            dto.UserId = int.Parse(userIdString);

            var result = await _bookingService.CreateAsync(dto);

            if (!result.IsSuccess) return BadRequest(new { message = result.Error });

            return Ok(new { message = "замовлення успішно створено.", bookings = result.Value });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "master, admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var result = await _bookingService.UpdateTaskStatusAsync(id, dto.Status);
            return result.IsSuccess ? Ok(new { message = "status upd" }) : BadRequest(new { message = result.Error });
        }
    }
}