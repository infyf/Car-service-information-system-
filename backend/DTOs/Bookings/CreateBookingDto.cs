using System.Text.Json.Serialization;

namespace AutoServ.DTOs.Bookings
{
    public class CreateBookingDto
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("customerName")]
        public string Name { get; set; } = "";

        [JsonPropertyName("customerPhone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("carPlate")]
        public string? CarNumber { get; set; }

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonPropertyName("bookingDate")]
        public string Date { get; set; } = "";

        [JsonPropertyName("bookingTime")]
        public string Time { get; set; } = "";

        [JsonPropertyName("totalPrice")]
        public decimal Total { get; set; }

        [JsonPropertyName("services")]
        public List<BookingServiceItemDto> Services { get; set; } = new();
    }

    public class BookingServiceItemDto
    {
        [JsonPropertyName("serviceItemId")]
        public int Id { get; set; }

        [JsonPropertyName("priceAtBooking")]
        public decimal Price { get; set; }

        [JsonPropertyName("duration")]
        public int DurationMinutes { get; set; } = 60;
    }
}