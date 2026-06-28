using System.ComponentModel.DataAnnotations;

namespace AutoServ.Core.Entities
{
    public class Recommendation
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }  // консультаційне бронювання
        public Booking Booking { get; set; } = null!;

        public int ServiceItemId { get; set; }
        public ServiceItem ServiceItem { get; set; } = null!;

        public string Comment { get; set; } = "";
        public bool IsAccepted { get; set; } = false;

        public int? CreatedBookingId { get; set; } // якщо клієнт прийняв рекомендацію
    }
}