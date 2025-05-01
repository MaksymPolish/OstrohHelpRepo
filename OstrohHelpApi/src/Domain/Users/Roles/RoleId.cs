namespace Domain.Users.Roles;

public record RoleId(Guid Value)
{
    public static RoleId New() => new(Guid.NewGuid());
    public static RoleId Empty() => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}