using Domain.Inventory.Statuses;
using Domain.Users;

namespace Domain.Inventory;

public class Questionary
{
    public Guid Id { get; set; }

    public User? User { get; set; }
    public Guid UserId { get; set; }

    public QuestionaryStatuses? Status { get; set; }
    public Guid StatusId { get; set; }
    public string Description { get; set; }
    public bool IsAnonymous { get; set; }
    public DateTime SubmittedAt { get; set; }

    public Questionary(Guid id, Guid userId, Guid statusId, string description,
        bool isAnonymous, DateTime submittedAt)
    {
        Id = id;
        UserId = userId;
        StatusId = statusId;
        Description = description;
        IsAnonymous = isAnonymous;
        SubmittedAt = submittedAt;
    }

    public static new Questionary Create(Guid id, Guid userId, Guid statusId,
        string description, bool isAnonymous, DateTime submittedAt) =>
        new(id, userId, statusId, description, isAnonymous, submittedAt);
}