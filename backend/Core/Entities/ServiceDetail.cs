using AutoServ.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoServ.Core.Entities
{
    public class ServiceDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;

        [Required]
        public decimal PriceFrom { get; set; }

        [Required]
        [MaxLength(50)]
        public string Duration { get; set; } = null!;

        [MaxLength(50)]
        public string? Warranty { get; set; }

        public Service Service { get; set; } = null!;
    }
}