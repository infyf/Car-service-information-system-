using System.ComponentModel.DataAnnotations;

namespace AutoServ.DTOs.Recommendations
{
    public class AcceptRecommendationDto
    {
        [Required]
        public int RecommendationId { get; set; }
        public DateTime? BookingDate { get; set; }
        public string? BookingTime { get; set; }
    }

    public class AcceptBatchRecommendationsDto
    {
        public List<int> RecommendationIds { get; set; } = new();
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTime { get; set; }
        public string TimeStrategy { get; set; } = "nearest";
    }


    public class BatchAcceptResultDto
    {
        public List<AcceptedItemDto> AcceptedItems { get; set; } = new();
        public List<FailedItemDto> FailedItems { get; set; } = new();
        public string Message { get; set; } = "";
    }

    public class AcceptedItemDto
    {
        public int RecommendationId { get; set; }
        public string ServiceTitle { get; set; } = "";
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public string MasterName { get; set; } = "";
    }

    public class FailedItemDto
    {
        public int RecommendationId { get; set; }
        public string ServiceTitle { get; set; } = "";
        public string Reason { get; set; } = "";
    }


    public class BatchPreviewDto
    {
        public string ProposedDate { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public List<PreviewItemDto> Items { get; set; } = new();
    }

    public class PreviewItemDto
    {
        public int RecommendationId { get; set; }
        public string ServiceTitle { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string MasterName { get; set; } = "";
        public decimal Price { get; set; }
    }
}