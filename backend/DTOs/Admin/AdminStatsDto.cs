namespace AutoServ.DTOs.Admin
{
    public class AdminStatsDto
    {
        public int TotalMasters { get; set; }
        public int ActiveMasters { get; set; }
        public int BusyMastersToday { get; set; }
        public int FreeMastersToday { get; set; }
    }
}