namespace AutoServ.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        public int TotalMasters { get; set; }
        public int ActiveMasters { get; set; }
        public int BusyMastersToday { get; set; }
        public int FreeMastersToday { get; set; }

     
        public int ScheduledToday { get; set; }
        public int PendingToday { get; set; }
        public int CompletedToday { get; set; }
    }
}