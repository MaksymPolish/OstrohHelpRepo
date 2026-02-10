namespace Api.Dtos;

public class AcceptQuestionnaireRequest
{
    public Guid QuestionaryId { get; set; }
    public Guid PsychologistId { get; set; }
    public DateTime ScheduledTime { get; set; }
}
