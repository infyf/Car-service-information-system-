using System.ComponentModel.DataAnnotations;

namespace AutoServ.DTOs.Admin
{
 
    public class ManageServiceDto
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Slug { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = null!;

        [Required]
        [Range(0.01, 999999)]
        public decimal PriceFrom { get; set; }

        public string? Duration { get; set; }
        public string? Warranty { get; set; }
    }

    public class ManageServiceItemDto
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        [Range(0.01, 999999)]
        public decimal PriceFrom { get; set; }

        [Required]
        [Range(5, 1440)] 
        public int DurationMinutes { get; set; } = 60; 
    }

    public class AdminServiceListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;


        public List<AdminServiceItemDto> Items { get; set; } = new();
    }


    public class AdminServiceItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal PriceFrom { get; set; }
        public int DurationMinutes { get; set; }
    }
    public class UpdateServiceDescriptionDto
    {
        public string Description { get; set; } = string.Empty;
    }
    public class UpdateServiceTitleDto
    {
        public string Title { get; set; } = string.Empty;
    }
}