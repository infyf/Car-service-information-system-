namespace AutoService.Api.DTOs;

public class ServiceItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int PriceFrom { get; set; }
}
