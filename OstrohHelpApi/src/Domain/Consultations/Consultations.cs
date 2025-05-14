using Domain.Consultations.Statuses;
using Domain.Inventory;
using Domain.Users;

namespace Domain.Consultations;

public class Consultations
{
    public ConsultationsId Id { get; set; }
    
    public Questionary Questionary { get; set; }
    public QuestionaryId? QuestionnaireId { get; set; }
    
    public User User { get; set; }
    public UserId StudentId { get; set; }
    public UserId PsychologistId { get; set; }
    public ConsultationStatuses ConsultationStatuses { get; set; }
    public ConsultationStatusesId StatusId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Consultations() => CreatedAt = DateTime.UtcNow;
    
    new Consultations Create(ConsultationsId id, QuestionaryId? questionnaireId, UserId studentId, UserId psychologistId,
        ConsultationStatusesId statusId, DateTime scheduledTime, DateTime createdAt) =>
        new(id, questionnaireId, studentId, psychologistId, statusId, scheduledTime, createdAt);

    Consultations(ConsultationsId id, QuestionaryId? questionnaireId, UserId studentId, UserId psychologistId,
        ConsultationStatusesId statusId, DateTime scheduledTime, DateTime createdAt)
    {
        Id = id;
        QuestionnaireId = questionnaireId;
        StudentId = studentId;
        PsychologistId = psychologistId;
        StatusId = statusId;
        ScheduledTime = scheduledTime;
        CreatedAt = createdAt;
    }
}