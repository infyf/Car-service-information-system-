using AutoServ.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AutoServ.Core.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? MasterId { get; set; }
        public Master? Master { get; set; }

        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }

        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";

        public string? Comment { get; set; }
        public string? CarPlate { get; set; }

        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string BookingType { get; set; } = "service";

        public int? ParentBookingId { get; set; }
        public Booking? ParentBooking { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<BookingService> Services { get; set; } = new List<BookingService>();
        public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();

        public virtual ICollection<Booking>? Children { get; set; }

        public string? PaymentId { get; set; } 
        public string? PaymentStatus { get; set; } = "pending"; 
        public DateTime? PaidAt { get; set; } 
    }
}