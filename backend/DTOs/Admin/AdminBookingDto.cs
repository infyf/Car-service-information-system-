using AutoServ.Core.Enums;

namespace AutoServ.DTOs.Admin
{
    public class AdminBookingDto
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingTime { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string ClientPhone { get; set; } = "";

        // Формат: "Toyota Camry AA1234BB 2020"
        public string CarInfo { get; set; } = "Не вказано";

        public string Services { get; set; } = "";
        public string MasterName { get; set; } = "Не призначено";
        public BookingStatus Status { get; set; }
        public string BookingType { get; set; } = "";
    }
}