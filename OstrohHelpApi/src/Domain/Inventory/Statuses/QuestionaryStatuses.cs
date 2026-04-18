namespace Domain.Inventory.Statuses;

public class QuestionaryStatuses
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public QuestionaryStatusEnum Status { get; set; }

    public static QuestionaryStatuses Create(Guid id, string name)
    {
        return new QuestionaryStatuses(id, name, MapNameToEnum(name));
    }

    public QuestionaryStatuses(Guid id, string name, QuestionaryStatusEnum status)
    {
        Id = id;
        Name = name;
        Status = status;
    }

    private static QuestionaryStatusEnum MapNameToEnum(string name)
    {
        return name switch
        {
            "Принято" => QuestionaryStatusEnum.Accepted,
            "Відхилено" => QuestionaryStatusEnum.Rejected,
            "Очікує підтвердження" => QuestionaryStatusEnum.Pending,
            _ => QuestionaryStatusEnum.Pending
        };
    }
}