namespace Domain.Inventory.Statuses;

public class QuestionaryStatuses
{
    public questionaryStatusId Id { get; set; }
    public string Name { get; set; }

    public static new QuestionaryStatuses Create(questionaryStatusId id, string name) => new(id, name);

    public QuestionaryStatuses(questionaryStatusId id, string name)
    {
        Id = id;
        Name = name;
    }
}