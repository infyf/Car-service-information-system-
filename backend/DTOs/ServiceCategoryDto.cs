namespace AutoService.Api.DTOs;

public class ServiceCategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public List<ServiceItemDto> Items { get; set; } = new();
}
