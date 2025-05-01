using Domain.Questionnaires.Statuses;
using Domain.Users;

namespace Domain.Questionnaires;

public class Questionnaire
{
    public QuestionnaireId Id { get; set; }
    
    public User User { get; set; }
    public UserId UserId { get; set; }
    
    public QuestionnaireStatuses Status { get; set; }
    public QuestionnaireStatusesId StatusId { get; set; }
    public string Description { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime SubmittedAt { get; set; }
    
    public Questionnaire(QuestionnaireId id, UserId userId, QuestionnaireStatusesId statusId, string description, bool isAnonymous, DateTime submittedAt)
    {
        Id = id;
        UserId = userId;
        StatusId = statusId;
        Description = description;
        IsAnonymous = isAnonymous;
        SubmittedAt = submittedAt;
    }
    
    new Questionnaire Create(QuestionnaireId id, UserId userId, QuestionnaireStatusesId statusId, string description, bool isAnonymous, DateTime submittedAt) =>
        new(id, userId, statusId, description, isAnonymous, submittedAt);
}