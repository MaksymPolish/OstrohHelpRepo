using Domain.Questionnaires;
using Domain.Users;

namespace Domain.Consultations;

public class Consultations
{
    public ConsultationsId Id { get; set; }
    public QuestionnaireId? QuestionnaireId { get; set; }
    public UserId StudentId { get; set; }
    public UserId PsychologistId { get; set; }
    public ConsultationStatusesId StatusId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }
}