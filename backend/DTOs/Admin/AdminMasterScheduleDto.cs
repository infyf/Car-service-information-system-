namespace AutoServ.DTOs.Admin
{
    public class AdminMasterScheduleDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public bool IsFree { get; set; }
        public List<AdminMasterSlotDto> Slots { get; set; } = new();
    }


    public class AdminMasterSlotDto
    {
        public string Start { get; set; } = "";
        public string End { get; set; } = "";
    }
}