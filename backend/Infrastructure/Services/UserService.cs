using AutoServ.Core.Entities;
using AutoServ.Core.Interfaces;
using AutoServ.Core.Models;
using AutoServ.DTOs.Auth;
using AutoServ.DTOs.Users;
using AutoServ.Infrastructure.Data;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoServ.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationContext _context;
        private readonly ITokenService _tokenService;
        private readonly string _googleClientId;

        public UserService(ApplicationContext context, ITokenService tokenService, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _googleClientId = configuration["GoogleAuth:ClientId"]
               ?? throw new Exception("GoogleAuth:ClientId не знайдено в конфігурації");
        }

        public async Task<Result<AuthResultDto>> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Result<AuthResultDto>.Failure("Email вже зареєстрований");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Phone = dto.Phone,
                    Role = "client",
                    CreatedAt = DateTime.UtcNow,
                    Profile = new UserProfile { CarBrand = "Не вказано", CarModel = "Не вказано", CarPlate = "Не вказано", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = MapToAuthResult(user);
                return Result<AuthResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<AuthResultDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<AuthResultDto>> RegisterMasterAsync(RegisterMasterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Result<AuthResultDto>.Failure("Email вже зареєстрований");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User { Email = dto.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), FirstName = dto.FirstName, LastName = dto.LastName, Phone = dto.Phone, Role = "master", CreatedAt = DateTime.UtcNow };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _context.Masters.Add(new Master { UserId = user.Id, FirstName = dto.FirstName, LastName = dto.LastName, Phone = dto.Phone, IsActive = true, ExperienceYears = 0 });
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<AuthResultDto>.Success(MapToAuthResult(user));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<AuthResultDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<AuthResultDto>> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Result<AuthResultDto>.Failure("Невірний email або пароль");

            bool isPasswordValid = false;
            if (user.PasswordHash.StartsWith("$2"))
                isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            else
            {
                using var sha = System.Security.Cryptography.SHA256.Create();
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dto.Password));
                isPasswordValid = user.PasswordHash == Convert.ToBase64String(bytes);
            }

            if (!isPasswordValid) return Result<AuthResultDto>.Failure("Невірний email або пароль");

            return Result<AuthResultDto>.Success(MapToAuthResult(user));
        }


        public async Task<Result<AuthResultDto>> GoogleLoginAsync(GoogleAuthDto dto)
        {
            try
            {
         
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleClientId }
                    });

                var email = payload.Email;
                if (string.IsNullOrEmpty(email))
                    return Result<AuthResultDto>.Failure("Google-токен не містить email");

               
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        PasswordHash = "google-oauth-no-password",  // заглушка
                        FirstName = payload.GivenName ?? "Користувач",
                        LastName = payload.FamilyName ?? "",
                        Phone = "Не вказано",
                        Role = "client",
                        CreatedAt = DateTime.UtcNow,
                        Profile = new UserProfile
                        {
                            CarBrand = "Не вказано",
                            CarModel = "Не вказано",
                            CarPlate = "Не вказано",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

       
                return Result<AuthResultDto>.Success(MapToAuthResult(user));
            }
            catch (InvalidJwtException)
            {
                return Result<AuthResultDto>.Failure("Невірний Google-токен");
            }
            catch (Exception ex)
            {
                return Result<AuthResultDto>.Failure("Помилка Google-авторизації: " + ex.Message);
            }
        }
        

        public async Task<Result<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Result<UserProfileDto>.Failure("Користувача не знайдено");

            var profileDto = new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                CarBrand = user.Profile?.CarBrand ?? "Не вказано",
                CarModel = user.Profile?.CarModel ?? "Не вказано",
                CarYear = user.Profile?.CarYear,
                CarEngine = user.Profile?.CarEngine ?? "Не вказано",
                CarPlate = user.Profile?.CarPlate ?? "Не вказано",
                CarImageUrl = user.Profile?.CarImageUrl,
                AvatarUrl = user.Profile?.AvatarUrl
            };

            return Result<UserProfileDto>.Success(profileDto);
        }

        public async Task<Result> UpdateProfileAsync(int userId, UserProfileDto dto)
        {
            var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Result.Failure("Користувача не знайдено");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Phone = dto.Phone;

            if (user.Profile == null)
            {
                user.Profile = new UserProfile { UserId = userId, CreatedAt = DateTime.UtcNow };
                _context.UserProfiles.Add(user.Profile);
            }

            user.Profile.CarBrand = dto.CarBrand;
            user.Profile.CarModel = dto.CarModel;
            user.Profile.CarYear = dto.CarYear;
            user.Profile.CarEngine = dto.CarEngine;
            user.Profile.CarPlate = dto.CarPlate;
            user.Profile.AvatarUrl = dto.AvatarUrl;
            user.Profile.CarImageUrl = dto.CarImageUrl;
            user.Profile.UpdatedAt = DateTime.UtcNow;

            try { await _context.SaveChangesAsync(); return Result.Success(); }
            catch (DbUpdateException ex) { return Result.Failure("Помилка бази даних: " + ex.InnerException?.Message); }
        }

        public async Task<Result<List<object>>> GetUserRecommendationsAsync(int userId)
        {
            var recommendations = await _context.Recommendations
                .Include(r => r.ServiceItem).Include(r => r.Booking)
                .Where(r => r.Booking.UserId == userId && r.IsAccepted == false).ToListAsync();

            var grouped = recommendations.GroupBy(r => r.BookingId)
                .Select(g => {
                    // formation of an array of service 
                    var items = g.Select(r => new
                    {
                        id = r.Id,
                        serviceItemId = r.ServiceItemId,
                        title = r.ServiceItem.Title,
                        price = r.ServiceItem.PriceFrom
                    })
                    .GroupBy(x => x.serviceItemId)
                    .Select(x => x.First())
                    .ToList();

                    return (object)new
                    {
                        bookingId = g.Key,
                        bookingDate = g.First().Booking.BookingDate.ToString("dd.MM.yyyy"),
                        bookingTime = g.First().Booking.BookingTime.ToString(@"hh\:mm"),
                        items = items
                    };
                }).ToList();

            return Result<List<object>>.Success(grouped);
        }

        private AuthResultDto MapToAuthResult(User user)
        {
            return new AuthResultDto
            {
                Token = _tokenService.GenerateJwtToken(user),
                User = new UserDto { Id = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, Phone = user.Phone, Role = user.Role.ToLowerInvariant() }
            };
        }
    }
}