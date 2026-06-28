namespace AutoServ.DTOs.Bookings
{
    public class TimeSlotDto
    {
        public string Time { get; set; } = "";
        public bool IsBooked { get; set; }
        public int? BookingId { get; set; }
    }

    public class MasterScheduleDto
    {
        public int MasterId { get; set; }
        public string MasterName { get; set; } = "";
        public DateTime Date { get; set; }
        public List<TimeSlotDto> Slots { get; set; } = new();
    }
}