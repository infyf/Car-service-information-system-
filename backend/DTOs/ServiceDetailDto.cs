namespace AutoService.Api.DTOs;

public class ServiceDetailDto
{
    public int ServiceId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public int PriceFrom { get; set; }
    public string Duration { get; set; } = null!;
    public string Warranty { get; set; } = null!;
}
