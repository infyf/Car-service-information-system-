using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Recommendations;
using AutoServ.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService) { _recommendationService = recommendationService; }

        [HttpPost]
        [Authorize(Roles = "master")]
        public async Task<IActionResult> Create([FromBody] CreateRecommendationDto dto)
        {
            try { return Ok(await _recommendationService.CreateAsync(dto)); }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            if (User.GetUserId() != userId && !User.IsInRole("admin")) return Forbid();
            var recs = await _recommendationService.GetByUserAsync(userId);
            return Ok(recs.Select(r => new { r.Id, r.ServiceItemId, Title = r.ServiceItem.Title, PriceFrom = r.ServiceItem.PriceFrom, r.Comment, BookingId = r.BookingId, BookingDate = r.Booking.BookingDate.ToString("dd.MM.yyyy"), BookingTime = r.Booking.BookingTime.ToString(@"hh\:mm"), Status = "pending" }));
        }

        [HttpPost("preview-batch")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> PreviewBatch([FromBody] AcceptBatchRecommendationsDto dto)
        {
            var result = await _recommendationService.PreviewBatchAsync(dto, User.GetUserId());
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPost("accept-batch")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> AcceptBatch([FromBody] AcceptBatchRecommendationsDto dto)
        {
            var result = await _recommendationService.AcceptBatchAsync(dto, User.GetUserId());
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }
    }
}