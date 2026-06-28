namespace AutoServ.DTOs.Masters
{
    public class MasterDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int ExperienceYears { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UpdateMasterSpecializationsDto
    {
        public List<int> ServiceItemIds { get; set; } = new List<int>();
    }

    public class MasterConsultationDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Problem { get; set; } = null!;
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public string Status { get; set; } = null!;


        public string? AttachmentUrl { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }
        public string? CarEngine { get; set; }
        public string? CarPlate { get; set; }
    }
}