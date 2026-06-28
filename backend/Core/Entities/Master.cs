using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServ.Core.Entities
{
    public class Master
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public int ExperienceYears { get; set; } = 0;

        [Column("user_id")]
        public int? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
    }
}