using Domain.Inventory.Statuses;
using Domain.Users;

namespace Domain.Inventory;

public class Questionary
{
    public QuestionaryId Id { get; set; }

    public User User { get; set; }
    public UserId UserId { get; set; }

    public QuestionaryStatuses Status { get; set; }
    public questionaryStatusId StatusId { get; set; }
    public string Description { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime SubmittedAt { get; set; }

    public Questionary(QuestionaryId id, UserId userId, questionaryStatusId statusId, string description,
        bool isAnonymous, DateTime submittedAt)
    {
        Id = id;
        UserId = userId;
        StatusId = statusId;
        Description = description;
        IsAnonymous = isAnonymous;
        SubmittedAt = submittedAt;
    }

    public static new Questionary Create(QuestionaryId id, UserId userId, questionaryStatusId statusId,
        string description, bool isAnonymous, DateTime submittedAt) =>
        new(id, userId, statusId, description, isAnonymous, submittedAt);
    
    internal void UpdateStatus(QuestionaryStatuses newStatus)
    {
        StatusId = newStatus.Id;
    }
}