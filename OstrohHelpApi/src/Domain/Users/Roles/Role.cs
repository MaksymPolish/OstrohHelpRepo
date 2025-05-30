﻿namespace Domain.Users.Roles;

public class Role
{
    public RoleId Id { get; set; }
    public string Name { get; set; }
    
    private Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public static new Role Create(RoleId id, string name) => new(id, name);
}