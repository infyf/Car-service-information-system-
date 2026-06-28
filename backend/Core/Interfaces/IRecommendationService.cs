using AutoServ.Core.Entities;
using AutoServ.Core.Models;
using AutoServ.DTOs.Recommendations;

namespace AutoServ.Core.Interfaces
{
    public interface IRecommendationService
    {
        Task<Recommendation> CreateAsync(CreateRecommendationDto dto);
        Task<List<Recommendation>> GetByBookingAsync(int bookingId);
        Task<List<Recommendation>> GetByUserAsync(int userId);

        Task<Result<object>> AcceptAsync(AcceptRecommendationDto dto, int userId);

        // Нові методи
        Task<Result<BatchPreviewDto>> PreviewBatchAsync(AcceptBatchRecommendationsDto dto, int userId);
        Task<Result<BatchAcceptResultDto>> AcceptBatchAsync(AcceptBatchRecommendationsDto dto, int userId);
    }
}