using Domain.Conferences;
using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Users;

namespace Api.Dtos;

public class ConsultationDto
{
    public ConsultationsId Id { get; set; }
    public QuestionaryId? QuestionnaireId { get; set; }
    public UserId StudentId { get; set; }
    public string StudentName { get; set; } = "Анонімно";
    public UserId PsychologistId { get; set; }
    public string PsychologistName { get; set; } = "Невідомий";
    public ConsultationStatusesId StatusId { get; set; }
    public string StatusName { get; set; } = "Невідомий";
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }
}