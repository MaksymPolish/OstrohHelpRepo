using Domain.Conferences.Statuses;
using Domain.Inventory;
using Domain.Users;

namespace Domain.Conferences;

public class Consultations
{
    public Guid Id { get; set; }

    public Questionary? Questionary { get; set; }
    public Guid? QuestionnaireId { get; set; }

    // Навігаційні властивості для вирішення N+1 проблеми
    public User? Student { get; set; }
    public Guid StudentId { get; set; }
    
    public User? Psychologist { get; set; }
    public Guid PsychologistId { get; set; }
    
    public ConsultationStatuses? Status { get; set; }
    public ConsultationStatusesId StatusId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime CreatedAt { get; set; }

    public Consultations() => CreatedAt = DateTime.UtcNow;
    
    public static Consultations Create(
        Guid id,
        Guid? questionnaireId,
        Guid studentId,
        Guid psychologistId,
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

    Consultations(Guid id, Guid? questionnaireId, Guid studentId, Guid psychologistId,
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