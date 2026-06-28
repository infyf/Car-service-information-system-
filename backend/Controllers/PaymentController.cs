using AutoServ.Core.Interfaces;
using AutoServ.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationContext _context;

        public PaymentController(IPaymentService paymentService, ApplicationContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        [HttpPost("generate")]
        [Authorize]
        public async Task<IActionResult> GeneratePayment([FromBody] PaymentRequestDto dto)
        {
       
            if (dto.OrderId <= 0)
            {
                return BadRequest(new { message = $"Неправильний ID замовлення: {dto.OrderId}" });
            }

            try
            {
          
                var initialBooking = await _context.Bookings.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == dto.OrderId);

                if (initialBooking == null)
                    return NotFound(new { message = $"Запис з ID {dto.OrderId} не існує" });

                int parentOrderId = initialBooking.ParentBookingId ?? initialBooking.Id;

                var parentBooking = await _context.Bookings.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == parentOrderId && b.BookingType == "service_order");

                if (parentBooking == null)
                    return NotFound(new { message = "Батьківське замовлення не знайдено" });

                var result = _paymentService.GeneratePaymentData(
                    parentBooking.Id,
                    parentBooking.TotalPrice,
                    $"Автосервіс: Замовлення #{parentBooking.Id}"
                );

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Error });

                return Ok(new { data = result.Value.Data, signature = result.Value.Signature });
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromForm] string data, [FromForm] string signature)
        {
            var result = _paymentService.ProcessCallback(data, signature);
            if (!result.IsSuccess) return Ok(new { status = "error", message = result.Error });

            int orderId = result.Value;

            var booking = await _context.Bookings.FindAsync(orderId);
            if (booking != null)
            {
                booking.Status = Core.Enums.BookingStatus.Confirmed;
                await _context.SaveChangesAsync();
            }

            return Ok(new { status = "success" });
        }
    }

    public class PaymentRequestDto
    {
        public int OrderId { get; set; }
    }
}