using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Users;

namespace Domain.Conferences;

public class Consultations
{
    public ConsultationsId Id { get; set; }

    public Questionary? Questionary { get; set; }
    public QuestionaryId? QuestionnaireId { get; set; }

    // Навігаційні властивості для вирішення N+1 проблеми
    public User? Student { get; set; }
    public UserId StudentId { get; set; }
    
    public User? Psychologist { get; set; }
    public UserId PsychologistId { get; set; }
    
    public ConsultationStatuses? Status { get; set; }
    public ConsultationStatusesId StatusId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }

    public Consultations() => CreatedAt = DateTime.UtcNow;
    
    public static Consultations Create(
        ConsultationsId id,
        QuestionaryId? questionnaireId,
        UserId studentId,
        UserId psychologistId,
        ConsultationStatusesId statusId,
        DateTime scheduledTime,
        DateTime createdAt)
    {
        return new Consultations(
            id,
            questionnaireId,
            studentId,
            psychologistId,
            statusId,
            scheduledTime,
            createdAt
        );
    }

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