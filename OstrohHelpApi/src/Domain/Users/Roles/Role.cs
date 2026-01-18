namespace Domain.Users.Roles;

public class Role
{
    // Parameterless constructor for AutoMapper and EF
    public Role() { }

    // Mapping between enum and Guid
    private static readonly Dictionary<RoleEnum, Guid> RoleEnumToGuid = new()
    {
        { RoleEnum.Student, new Guid("00000000-0000-0000-0000-000000000001") },
        { RoleEnum.Psychologist, new Guid("00000000-0000-0000-0000-000000000002") },
        { RoleEnum.HeadOfService, new Guid("00000000-0000-0000-0000-000000000003") },
    };

    private static readonly Dictionary<Guid, RoleEnum> GuidToRoleEnum = RoleEnumToGuid.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static Guid GetGuidByEnum(RoleEnum roleEnum)
    {
        return RoleEnumToGuid.TryGetValue(roleEnum, out var guid) ? guid : Guid.Empty;
    }

    public static RoleEnum GetEnumByGuid(Guid guid)
    {
        return GuidToRoleEnum.TryGetValue(guid, out var roleEnum) ? roleEnum : RoleEnum.Student;
    }

    public RoleId? Id { get; set; }
    public string? Name { get; set; }

    private Role(RoleId id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Role Create(RoleId id, string name) => new Role(id, name);
}