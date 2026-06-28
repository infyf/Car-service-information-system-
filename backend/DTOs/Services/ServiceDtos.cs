namespace AutoServ.DTOs.Services
{
    public class ServiceListDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Icon { get; set; } = null!;
    }

    public class ServiceItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal PriceFrom { get; set; }
    }

    public class ServiceDetailDto
    {
        public int ServiceId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public decimal PriceFrom { get; set; } // ТУТ БУЛО INT, СТАЛО DECIMAL
        public string Duration { get; set; } = null!;
        public string Warranty { get; set; } = null!;
    }

    public class ServiceCategoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public List<ServiceItemDto> Items { get; set; } = new();
    }

    public class CreateServiceDto
    {
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
    }
}