using System.ComponentModel.DataAnnotations;

namespace AutoServ.Core.Entities
{
    public class BookingService
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int ServiceItemId { get; set; }
        public ServiceItem ServiceItem { get; set; } = null!;

        public decimal PriceAtBooking { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}