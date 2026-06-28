using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServ.Core.Entities
{
    public class Consultation
    {
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("master_id")]
        public int? MasterId { get; set; }

        [ForeignKey(nameof(MasterId))]
        public Master? Master { get; set; }

        [Column("problem_description")]
        public string ProblemDescription { get; set; } = string.Empty;

        [Column("consultation_date")]
        public DateOnly ConsultationDate { get; set; }

        [Column("consultation_time")]
        public TimeOnly ConsultationTime { get; set; }

        [Column("status")]
        public string Status { get; set; } = "new";


        [Column("attachment_url")]
        public string? AttachmentUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}