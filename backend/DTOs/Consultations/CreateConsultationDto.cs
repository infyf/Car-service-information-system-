namespace AutoServ.DTOs.Consultations
{
    public class CreateConsultationDto
    {
        public int UserId { get; set; }
        public string ProblemDescription { get; set; } = null!;
        public DateOnly ConsultationDate { get; set; }
        public TimeOnly ConsultationTime { get; set; }
    }
}