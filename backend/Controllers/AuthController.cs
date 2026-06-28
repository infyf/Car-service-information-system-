using AutoServ.Core.Interfaces;
using AutoServ.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServ.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService) { _userService = userService; }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPost("register-master")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RegisterMaster([FromBody] RegisterMasterDto dto)
        {
            var result = await _userService.RegisterMasterAsync(dto);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { message = result.Error });
        }

        // 
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.IdToken))
                return BadRequest(new { message = "IdToken обов'язковий" });

            var result = await _userService.GoogleLoginAsync(dto);
            return result.IsSuccess
                ? Ok(result.Value)
                : Unauthorized(new { message = result.Error });
        }
        
    }
}