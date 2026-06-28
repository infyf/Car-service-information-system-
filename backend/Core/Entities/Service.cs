namespace AutoServ.Core.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public ServiceDetail ServiceDetail { get; set; }
        public ICollection<ServiceItem> Items { get; set; } = new List<ServiceItem>();
    }
}
