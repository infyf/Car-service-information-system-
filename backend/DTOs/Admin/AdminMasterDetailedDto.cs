namespace AutoServ.DTOs.Admin
{
    public class AdminMasterDetailedDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int ExperienceYears { get; set; }
        public bool IsActive { get; set; }


        public bool IsBusyToday { get; set; }
        public int TotalCompletedBookings { get; set; }


        public List<MasterSpecDto> Specializations { get; set; } = new();
    }

    public class MasterSpecDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }
}