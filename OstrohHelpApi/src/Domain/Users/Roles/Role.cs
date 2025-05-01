namespace Domain.Users.Roles;

public class Role
{
    public RoleId Id { get; set; }
    public string Name { get; set; }
    
    public Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
    }
}