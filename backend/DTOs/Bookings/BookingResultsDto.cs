using AutoServ.Core.Enums;

namespace AutoServ.DTOs.Bookings
{
    public class BookingHistoryDto
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string? CarPlate { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string BookingType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string MasterName { get; set; } = "Не призначено";
        public List<HistoryServiceDto> Services { get; set; } = new();
        public int? ParentBookingId { get; set; }
        public string ServiceDisplay { get; set; } = "";     
        public string? AttachmentUrl { get; set; }           
    }

    public class HistoryServiceDto
    {
        public int ServiceItemId { get; set; }
        public string Title { get; set; } = "N/A";
        public decimal PriceAtBooking { get; set; }
    }

    public class MasterScheduleTaskDto
    {
        public int Id { get; set; }
        public string Time { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Car { get; set; } = null!;
        public string Service { get; set; } = null!;
        public string Duration { get; set; } = null!;
        public string Price { get; set; } = null!;
        public BookingStatus Status { get; set; }
    }

    public class BookingCreatedResultDto
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CreatedTaskDto> Tasks { get; set; } = new();
    }

    public class CreatedTaskDto
    {
        public int TaskId { get; set; }
        public int MasterId { get; set; }
        public string MasterName { get; set; } = null!;
        public string Time { get; set; } = null!;
    }
}