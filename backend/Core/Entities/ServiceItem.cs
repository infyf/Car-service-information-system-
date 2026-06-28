namespace AutoServ.Core.Entities
{
    public class ServiceItem
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Title { get; set; } = null!;
        public decimal PriceFrom { get; set; }

        public int DurationMinutes { get; set; } = 60; 

        public Service Service { get; set; } = null!;

        
        public ICollection<Master> Masters { get; set; } = new List<Master>();
    }
}