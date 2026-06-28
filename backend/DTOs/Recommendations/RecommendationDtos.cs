using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoServ.DTOs.Recommendations
{
    public class CreateRecommendationDto
    {
        [JsonPropertyName("bookingId")]
        public int BookingId { get; set; }

        [JsonPropertyName("serviceItemId")]
        public int ServiceItemId { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}