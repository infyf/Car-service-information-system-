using AutoServ.Core.Models;
using AutoServ.DTOs.Auth;
using AutoServ.DTOs.Users;

namespace AutoServ.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto);
        Task<Result<AuthResultDto>> RegisterMasterAsync(RegisterMasterDto dto);
        Task<Result<AuthResultDto>> LoginAsync(LoginDto dto);
        Task<Result<UserProfileDto>> GetProfileAsync(int userId);
        Task<Result> UpdateProfileAsync(int userId, UserProfileDto dto);
        Task<Result<List<object>>> GetUserRecommendationsAsync(int userId);
        Task<Result<AuthResultDto>> GoogleLoginAsync(GoogleAuthDto dto);


    }
}
