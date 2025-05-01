using Domain.Users;

namespace Domain.Questionnaires;

public class Questionnaire
{
    public QuestionnaireId Id { get; set; }
    public UserId UserId { get; set; }
    public QuestionnaireStatusesId StatusId { get; set; }
    public string Description { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime SubmittedAt { get; set; }
}