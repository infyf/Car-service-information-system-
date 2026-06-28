namespace AutoServ.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Role { get; set; } = "client"; 

        
        public UserProfile? Profile { get; set; }


        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}