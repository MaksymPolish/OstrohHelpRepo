namespace Domain.Users.Tockens;

public record UserTockenId(Guid Value)
{
    public static UserTockenId New() => new(Guid.NewGuid());
    public static UserTockenId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}