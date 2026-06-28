namespace AutoServ.DTOs.Admin
{
    public class AdminClientDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public string CarInfo { get; set; } = "Не вказано";

        public DateTime RegisteredAt { get; set; }
        public int TotalBookings { get; set; }
    }
}