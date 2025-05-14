namespace Domain.Inventory.Statuses;

public record questionaryStatusId(Guid Value)
{
    public static questionaryStatusId New() => new(Guid.NewGuid());
    public static questionaryStatusId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}