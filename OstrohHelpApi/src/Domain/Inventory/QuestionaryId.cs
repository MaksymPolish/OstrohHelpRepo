namespace Domain.Inventory;

public record QuestionaryId(Guid Value)
{
    public static QuestionaryId New() => new(Guid.NewGuid());
    public static QuestionaryId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}