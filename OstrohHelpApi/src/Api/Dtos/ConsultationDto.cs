using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Users;

namespace Api.Dtos;

public class ConsultationDto
{
    public string Id { get; set; } = null!;
    public string StudentId { get; set; } = null!;
    public string PsychologistId { get; set; } = null!;
    public string StatusName { get; set; } = "Невідомий";
    public string StudentName { get; set; } = "Невідомий";
    public string PsychologistName { get; set; } = "Невідомий";
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }
}