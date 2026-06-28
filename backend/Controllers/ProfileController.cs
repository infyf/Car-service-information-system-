using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoServ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFileStorageService _fileStorageService;


        public ProfileController(IUserService userService, IFileStorageService fileStorageService)
        {
            _userService = userService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            if (!HasAccess(userId)) return Forbid();

            var result = await _userService.GetProfileAsync(userId);
            return result.IsSuccess ? Ok(result.Value) : NotFound(new { message = result.Error });
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UserProfileDto dto)
        {
            if (!HasAccess(userId)) return Forbid();

            var result = await _userService.UpdateProfileAsync(userId, dto);
            return result.IsSuccess ? Ok(new { message = "Профіль оновлено" }) : StatusCode(500, new { message = result.Error });
        }

        
        [HttpPost("{userId}/uploadCar")]
        public async Task<IActionResult> UploadCarImage(int userId, IFormFile file)
        {
            if (!HasAccess(userId)) return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не вибрано." });

      
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Файл занадто великий (макс. 5MB)." });

            using var stream = file.OpenReadStream();

            var relativePath = await _fileStorageService.UploadFileAsync(stream, "cars", file.FileName);

            return Ok(new { url = relativePath });
        }

        [HttpGet("{userId}/recommendations")]
        public async Task<IActionResult> GetUserRecommendations(int userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId.ToString()) return Forbid();

            var result = await _userService.GetUserRecommendationsAsync(userId);
            return Ok(result.Value);
        }

        private bool HasAccess(int targetUserId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return currentUserId == targetUserId.ToString() || User.IsInRole("admin");
        }
    }
}