namespace AutoServ.DTOs.Admin
{
    public class AdminScheduleTaskDto
    {
        public int Id { get; set; }
        public string MasterName { get; set; } = "";
        public string Time { get; set; } = "";
        public string EndTime { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string CarInfo { get; set; } = "";
        public string Service { get; set; } = "";
        public string Status { get; set; } = "";
    }
}